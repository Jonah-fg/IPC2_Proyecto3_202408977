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
            _aplicadorPagos =aplicadorPagos;
        }

        [HttpPost("cargar")]
        public IActionResult CargarTransacciones(IFormFile archivo)
        {
            if (archivo == null || archivo.Length== 0)
                return BadRequest("<error>Archivo vacío</error>");

            try
            {
                List<Cliente> clientes= _almacenamiento.CargarClientes();
                List<Banco> bancos= _almacenamiento.CargarBancos();
                List<Factura> facturas= _almacenamiento.CargarFacturas();
                List<Pago> pagos=_almacenamiento.CargarPagos();

                int nuevasFacturas=0;
                int facturasDuplicadas = 0;
                int facturasConError =0;
                int nuevosPagos = 0;
                int pagosDuplicados =0;
                int pagosConError=0;

                XmlDocument doc=new XmlDocument();
                doc.Load(archivo.OpenReadStream());

                // facturas
                XmlNodeList nodosFacturas= doc.SelectNodes("/transacciones/facturas/factura");
                if (nodosFacturas != null)
                {
                    foreach (XmlNode nodo in nodosFacturas)
                    {
                        string numeroFactura =nodo.SelectSingleNode("numeroFactura")?.InnerText ?? "";
                        string nitCliente =nodo.SelectSingleNode("NITcliente")?.InnerText ?? "";
                        string fechaStr=nodo.SelectSingleNode("fecha")?.InnerText ?? "";
                        string valorStr=nodo.SelectSingleNode("valor")?.InnerText ?? "";

                         if (string.IsNullOrEmpty(numeroFactura) || string.IsNullOrEmpty(nitCliente) ||  string.IsNullOrEmpty(fechaStr) || string.IsNullOrEmpty(valorStr))
                        {
                            facturasConError++;
                            continue;
                        } 

                        if (!clientes.Any(c =>c.Nit==nitCliente))
                        {
                            facturasConError++;
                            continue;
                        }

                        DateTime fecha;
                        decimal monto;
                        try
                        {
                            fecha=DateTime.ParseExact(fechaStr, "dd/MM/yyyy", null);
                            monto= decimal.Parse(valorStr);
                        }
                        catch
                        {
                            facturasConError++;
                            continue;
                        }

                        if (facturas.Any(f => f.NumeroFactura==numeroFactura))
                        {
                            facturasDuplicadas++;
                            continue;
                        }

                        Factura nueva= new Factura{NumeroFactura= numeroFactura, NitCliente = nitCliente, Fecha = fecha, Monto =monto, SaldoPendiente=monto, Pagada =false};
                        facturas.Add(nueva);
                        nuevasFacturas++;
                    }
                }

                // Pagos
                XmlNodeList nodosPagos =doc.SelectNodes("/transacciones/pagos/pago");
                if (nodosPagos!= null)
                {
                    foreach (XmlNode nodo in nodosPagos)
                    {
                        string codigoBanco =nodo.SelectSingleNode("codigoBanco")?.InnerText?? "";
                        string fechaStr=nodo.SelectSingleNode("fecha")?.InnerText ?? "";
                        string nitCliente = nodo.SelectSingleNode("NITcliente")?.InnerText ?? "";
                        string valorStr = nodo.SelectSingleNode("valor")?.InnerText?? "";

                        if (string.IsNullOrEmpty(codigoBanco) || string.IsNullOrEmpty(nitCliente) || string.IsNullOrEmpty(fechaStr) || string.IsNullOrEmpty(valorStr))
                        {
                            pagosConError++;
                            continue;
                        }

                        if (!clientes.Any(c => c.Nit==nitCliente) || !bancos.Any(b => b.Codigo == codigoBanco))
                        {
                            pagosConError++;
                            continue;
                        }

                        DateTime fecha;
                        decimal monto;
                        try
                        {
                            fecha= DateTime.ParseExact(fechaStr, "dd/MM/yyyy", null);
                            monto=decimal.Parse(valorStr);
                        }
                        catch
                        {
                            pagosConError++;
                            continue;
                        }

                        bool duplicado=pagos.Any(p => p.NitCliente ==nitCliente && p.CodigoBanco == codigoBanco && p.Fecha==fecha && p.Monto== monto);
                        if (duplicado)
                        {
                            pagosDuplicados++;
                            continue;
                        }

                        Pago nuevoPago= new Pago {NitCliente=nitCliente, CodigoBanco=codigoBanco, Fecha = fecha, Monto =monto, Referencia= $"Pago {codigoBanco} {fecha:yyyyMMdd}"};
                        pagos.Add(nuevoPago);
                        nuevosPagos++;

                        // Aplicar pago a facturas del cliente
                        Cliente cliente=clientes.First(c => c.Nit==nitCliente);
                        ResultadoAplicacionPago resultado= _aplicadorPagos.AplicarPago(nitCliente, monto, facturas);
                        cliente.SaldoFavor += resultado.SaldoFavor;
                    }
                }
                _almacenamiento.GuardarClientes(clientes);
                _almacenamiento.GuardarFacturas(facturas);
                _almacenamiento.GuardarPagos(pagos);

                string xmlRespuesta= $@"<?xml version=""1.0""?>
<transacciones>
  <facturas>
    <nuevasFacturas>{nuevasFacturas}</nuevasFacturas>
    <facturasDuplicadas>{facturasDuplicadas}</facturasDuplicadas>
    <facturasConError>{facturasConError}</facturasConError>
  </facturas>
  <pagos>
    <nuevosPagos>{nuevosPagos}</nuevosPagos>
    <pagosDuplicados>{pagosDuplicados}</pagosDuplicados>
    <pagosConError>{pagosConError}</pagosConError>
  </pagos>
</transacciones>";

                return Content(xmlRespuesta, "application/xml");
            }
            catch (Exception ex)
            {
                return BadRequest($"<error>{ex.Message}</error>");
            }
        }
    }
}