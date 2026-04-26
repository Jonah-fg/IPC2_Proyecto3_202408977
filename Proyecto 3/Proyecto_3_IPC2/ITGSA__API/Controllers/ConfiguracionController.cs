using ITGSA.API.Services;
using ITGSA__API.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace ITGSA__API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly AlmacenamientoXml _almacenamiento;

        public ConfiguracionController(AlmacenamientoXml almacenamiento)
        {
            _almacenamiento=almacenamiento;
        }

        [HttpPost("cargar")]
        public IActionResult CargarConfiguracion(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("<error>Archivo vacío</error>");

            try
            {
                XmlDocument doc =new XmlDocument();
                doc.Load(archivo.OpenReadStream());

                List<Cliente> clientes=_almacenamiento.CargarClientes();
                List<Banco> bancos =_almacenamiento.CargarBancos();

                int clientesAgreg = 0;
                int clientesAct = 0;
                int bancosAgreg= 0;
                int bancosAct=0;

                // Clientes
                XmlNodeList nodosClientes= doc.SelectNodes("/config/clientes/cliente");
                if (nodosClientes !=null)
                {
                    foreach (XmlNode nodo in nodosClientes)
                    {
                        string nit =nodo.SelectSingleNode("nit")?.InnerText ?? "";
                        string nombre= nodo.SelectSingleNode("nombre")?.InnerText ?? "";
                        string direccion = nodo.SelectSingleNode("direccion")?.InnerText ?? "";
                        if (nit=="") 
                            continue;

                        Cliente clienteExiste= clientes.Find(c => c.Nit == nit);
                        if (clienteExiste !=null)
                        {
                            clienteExiste.Nombre= nombre;
                            clienteExiste.Direccion=direccion;
                            clientesAct++;
                        }
                        else
                        {
                            clientes.Add(new Cliente { Nit =nit, Nombre =nombre, Direccion = direccion, SaldoFavor=0 });
                            clientesAgreg++;
                        }
                    }
                }

                // Bancos
                XmlNodeList nodosBancos=doc.SelectNodes("/config/bancos/banco");
                if (nodosBancos != null)
                {
                    foreach (XmlNode nodo in nodosBancos)
                    {
                        string codigo= nodo.SelectSingleNode("codigo")?.InnerText ?? "";
                        string nombre=nodo.SelectSingleNode("nombre")?.InnerText ?? "";
                        if (codigo=="") 
                            continue;

                        Banco existe=bancos.Find(b => b.Codigo==codigo);
                        if (existe !=null)
                        {
                            existe.Nombre= nombre;
                            bancosAct++;
                        }
                        else
                        {
                            bancos.Add(new Banco {Codigo = codigo, Nombre = nombre});
                            bancosAgreg++;
                        }
                    }
                }

                _almacenamiento.GuardarClientes(clientes);
                _almacenamiento.GuardarBancos(bancos);

                string xmlRespuesta=$@"<?xml version=""1.0""?>
<respuesta>
  <clientesAgregados>{clientesAgreg}</clientesAgregados>
  <clientesActualizados>{clientesAct}</clientesActualizados>
  <bancosAgregados>{bancosAgreg}</bancosAgregados>
  <bancosActualizados>{bancosAct}</bancosActualizados>
</respuesta>";

                return Content(xmlRespuesta, "application/xml");
            }
            catch (Exception ex)
            {
                return BadRequest($"<error>{ex.Message}</error>");
            }
        }
    }
}
