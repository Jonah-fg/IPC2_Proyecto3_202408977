using ITGSA.API.Services;
using ITGSA__API.Modelos;
using ITGSA__API.Servicios;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace ITGSA__API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionesController : ControllerBase
    {
        private readonly AlmacenamientoXml _almacenamiento;
        private readonly AplicadorPagos _aplicadorPagos;

        public TransaccionesController(AlmacenamientoXml almacenamiento, AplicadorPagos aplicadorPagos)
        {
            _almacenamiento =almacenamiento;
            _aplicadorPagos=aplicadorPagos;
        }

        [HttpPost("cargar")]
        public IActionResult CargarTransacciones(IFormFile archivo)
        {
            if (archivo ==null || archivo.Length==0)
                return BadRequest("<error>Archivo vacío</error>");

            try
            {
                XmlDocument doc =new XmlDocument();
                doc.Load(archivo.OpenReadStream());

                List<Cliente> clientes =_almacenamiento.CargarClientes();
                List<Factura> facturas =_almacenamiento.CargarFacturas();
                List<Pago> pagos = _almacenamiento.CargarPagos();
                int facturasProc =0;
                int pagosProc=0;

                // Facturas
                XmlNodeList facturasNode= doc.SelectNodes("/transacciones/facturas/factura");
                if (facturasNode != null)
                {
                    foreach (XmlNode nodo in facturasNode)
                    {
                        string nit= nodo.SelectSingleNode("nitCliente")?.InnerText ?? "";
                        string numStr=nodo.SelectSingleNode("numeroFactura")?.InnerText ?? "";
                        string fechaStr= nodo.SelectSingleNode("fecha")?.InnerText ?? "";
                        string montoStr= nodo.SelectSingleNode("monto")?.InnerText ?? "";

                        if (nit ==""|| numStr=="")
                            continue;

                        int numero =int.Parse(numStr);
                        DateTime fecha= DateTime.Parse(fechaStr);
                        decimal monto=decimal.Parse(montoStr);

                        bool yaExiste=facturas.Exists(f => f.NumeroFactura == numero); 
                        if (yaExiste)
                            continue;

                        Cliente cliente= clientes.Find(c => c.Nit==nit);
                        if (cliente==null) 
                            continue;

                        Factura facturaNueva =new Factura{NitCliente= nit, NumeroFactura=numero, Fecha= fecha, Monto =monto, SaldoPendiente=monto,  Pagada =false};

                        // Aplicar saldo a favor (si tiene)
                        if (cliente.SaldoFavor>0)
                        {
                            if (cliente.SaldoFavor>= monto)
                            {
                                facturaNueva.SaldoPendiente=0;
                                facturaNueva.Pagada =true;
                                cliente.SaldoFavor  -=monto;
                            }
                            else
                            {
                                facturaNueva.SaldoPendiente= monto-cliente.SaldoFavor;
                                cliente.SaldoFavor =0;
                            }
                        }
                        facturas.Add(facturaNueva);
                        facturasProc++;
                    }
                }

                // Pagos
                XmlNodeList pagosNode =doc.SelectNodes("/transacciones/pagos/pago");
                if (pagosNode !=null)
                {
                    foreach (XmlNode nodo in pagosNode)
                    {
                        string nit =nodo.SelectSingleNode("nitCliente")?.InnerText ?? "";
                        string fechaStr=nodo.SelectSingleNode("fecha")?.InnerText ?? "";
                        string montoStr=nodo.SelectSingleNode("monto")?.InnerText ?? "";
                        string refe =nodo.SelectSingleNode("referencia")?.InnerText ?? "";

                        if (nit == "" || montoStr == "") 
                            continue;

                        decimal monto =decimal.Parse(montoStr);
                        DateTime fecha= DateTime.Parse(fechaStr);

                        Cliente cliente =clientes.Find(c => c.Nit == nit);
                        if (cliente ==null) 
                            continue;

                        var facturasCliente =facturas.Where(f => f.NitCliente == nit && !f.Pagada).OrderBy(f => f.Fecha).ToList();
                        decimal resto =monto;
                        foreach (Factura fac in facturasCliente)
                        {
                            if (resto<= 0) 
                                break;

                            if (resto >= fac.SaldoPendiente)
                            {
                                resto -= fac.SaldoPendiente;
                                fac.SaldoPendiente = 0;
                                fac.Pagada = true;
                            }
                            else
                            {
                                fac.SaldoPendiente -= resto;
                                resto = 0;
                            }
                        }
                        cliente.SaldoFavor+=resto;

                        pagos.Add(new Pago { NitCliente = nit, Fecha = fecha, Monto = monto, Referencia = refe });
                        pagosProc++;
                    }
                }
                _almacenamiento.GuardarClientes(clientes);
                _almacenamiento.GuardarFacturas(facturas);
                _almacenamiento.GuardarPagos(pagos);

                string respuesta=$@"<respuesta>
    <facturasProcesadas>{facturasProc}</facturasProcesadas>
    <pagosProcesados>{pagosProc}</pagosProcesados>
    </respuesta>";
                return Content(respuesta, "application/xml");
            }
            catch (Exception ex)
            {
                return BadRequest($"<error>{ex.Message}</error>");
            }
        }
    }
}