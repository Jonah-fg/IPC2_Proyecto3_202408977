using ITGSA.API.Services;
using ITGSA__API.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class ConsultasController : ControllerBase
{
    private readonly AlmacenamientoXml _almacenamiento;

    public ConsultasController(AlmacenamientoXml almacenamiento)
    {
        _almacenamiento =almacenamiento;
    }

    [HttpGet("estadoCuenta")]
    public IActionResult EstadoCuenta(string nit)
    {
        List<Cliente> clientes=_almacenamiento.CargarClientes();
        List<Factura> facturas=_almacenamiento.CargarFacturas();
        List<Pago> pagos=_almacenamiento.CargarPagos();

        if (!string.IsNullOrEmpty(nit))
        {
            Cliente clien =clientes.Find(x =>x.Nit == nit);
            if (clien==null) 
                return NotFound("<error>Cliente no existe</error>");

            string xml=GenerarXmlCliente(clien, facturas, pagos);
            return Content(xml, "application/xml");
        }
        else
        {
            List<Cliente> ordenados=clientes.OrderBy(c =>c.Nit).ToList();
            StringBuilder sb=new StringBuilder();
            sb.AppendLine("<estadosCuenta>");
            foreach (Cliente clien in ordenados)
            {
                sb.AppendLine(GenerarXmlCliente(clien, facturas, pagos));
            }
            sb.AppendLine("</estadosCuenta>");
            return Content(sb.ToString(), "application/xml");
        }
    }

    private string GenerarXmlCliente(Cliente clien, List<Factura> facturas, List<Pago> pagos)
    {
        List<Transaccion> transacciones =new List<Transaccion>();
        foreach (Factura fac in facturas.Where(f =>f.NitCliente == clien.Nit))
        {
            transacciones.Add(new Transaccion { Fecha =fac.Fecha, Cargo= fac.Monto, Abono=0, Descripcion = $"Factura #{fac.NumeroFactura}" });
        }

        foreach (Pago p in pagos.Where(p =>p.NitCliente==clien.Nit))
        {
            transacciones.Add(new Transaccion { Fecha=p.Fecha, Cargo= 0, Abono=p.Monto, Descripcion =$"Pago {p.Referencia}" });
        }
        transacciones=transacciones.OrderByDescending(t => t.Fecha).ToList();

        StringBuilder sb= new StringBuilder();
        sb.AppendLine($"<cliente nit=\"{clien.Nit}\" nombre=\"{clien.Nombre}\" saldoActual=\"{clien.SaldoFavor:F2}\">");
        sb.AppendLine("  <transacciones>");
        foreach (Transaccion trans in transacciones)
        {
            sb.AppendLine($"   <transaccion fecha=\"{trans.Fecha:yyyy-MM-dd}\" cargo=\"{trans.Cargo:F2}\" abono=\"{trans.Abono:F2}\" descripcion=\"{trans.Descripcion}\"/>");
        }
        sb.AppendLine("  </transacciones>");
        sb.AppendLine("</cliente>");
        return sb.ToString();
    }

    [HttpGet("ingresos")]
    public IActionResult Ingresos(int mes, int anio)
    {
        List<Pago> pagos =_almacenamiento.CargarPagos();
        var resultado= new List<string>();
        for (int i =0; i<3; i++)
        {
            DateTime fecha =new DateTime(anio, mes, 1).AddMonths(-i);
            decimal total =0;
            foreach (Pago p in pagos)
            {
                if (p.Fecha.Year== fecha.Year && p.Fecha.Month== fecha.Month)
                    total+= p.Monto;
            }
            resultado.Add($"<mes nombre=\"{fecha:MMMM/yyyy}\" total=\"{total:F2}\"/>");
        }
        string xml="<ingresos>\n" + string.Join("\n", resultado) + "\n</ingresos>";
        return Content(xml, "application/xml");
    }
}


