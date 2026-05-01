using ITGSA__Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Diagnostics;

namespace ITGSA__Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetearDatos()
        {
            try
            {
                HttpClient cliente=_httpClientFactory.CreateClient("ClienteAPI");
                HttpResponseMessage respuesta =await cliente.PostAsync("/api/Reinicio", null);
                if (respuesta.IsSuccessStatusCode)
                {
                    string contenido =await respuesta.Content.ReadAsStringAsync();
                    ViewBag.Mensaje ="Datos reiniciados correctamente";
                    ViewBag.RespuestaXml=contenido;
                }
                else
                {
                    ViewBag.Error="Error al reiniciar datos";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error =$"Error de conexión: {ex.Message}";
            }
            return View("Index");
        }

        public IActionResult CargarTransacciones()
        {
            if (TempData["UltimaRespuestaTransac"] is string respuesta)
            {
                ViewBag.RespuestaXml =respuesta;
                TempData.Keep("UltimaRespuestaTransac"); 
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CargarTransacciones(IFormFile archivoXml)
        {
            if (archivoXml ==null || archivoXml.Length==0)
            {
                ViewBag.Error ="Debe selecionar un archivo transac.xml";
                return View();
            }

            HttpClient cliente= _httpClientFactory.CreateClient("ClienteAPI");
            using (var contenido =new MultipartFormDataContent())
            {
                var streamArchivo=archivoXml.OpenReadStream();
                var contenidoArchivo =new StreamContent(streamArchivo);
                contenido.Add(contenidoArchivo, "archivo", archivoXml.FileName);

                HttpResponseMessage respuesta =await cliente.PostAsync("/api/Transacciones/cargar", contenido);
                string xmlRespuesta =await respuesta.Content.ReadAsStringAsync();
                TempData["UltimaRespuestaTransac"] = xmlRespuesta;
                ViewBag.RespuestaXml=xmlRespuesta;
            }
            return View();
        }


        public IActionResult CargarConfiguracion()
        {
            if (TempData["UltimaRespuestaConfig"] is string respuesta)
            {
                ViewBag.RespuestaXml= respuesta;
                TempData.Keep("UltimaRespuestaConfig");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CargarConfiguracion(IFormFile archivoXml)
        {
            if (archivoXml ==null || archivoXml.Length== 0)
            {
                ViewBag.Error="Debe seleccionar un archivo config.xml";
                return View();
            }

            HttpClient cliente =_httpClientFactory.CreateClient("ClienteAPI");
            using (MultipartFormDataContent contenido=new MultipartFormDataContent())
            {
                Stream streamArchivo = archivoXml.OpenReadStream();
                StreamContent contenidoArchivo = new StreamContent(streamArchivo);
                contenido.Add(contenidoArchivo, "archivo", archivoXml.FileName);

                HttpResponseMessage respuesta=await cliente.PostAsync("/api/Configuracion/cargar", contenido);
                string xmlRespuesta =await respuesta.Content.ReadAsStringAsync();
                TempData["UltimaRespuestaConfig"] = xmlRespuesta;
                ViewBag.RespuestaXml = xmlRespuesta;
            }
            return View();
        }

        // GET: EstadoCuenta
        public async Task<IActionResult> EstadoCuenta(string nit = null)
        {
            HttpClient cliente= _httpClientFactory.CreateClient("ClienteAPI");
            string url="/api/Consultas/estadoCuenta";
            if (!string.IsNullOrEmpty(nit))
                url +=$"?nit={nit}";

            HttpResponseMessage respuesta = await cliente.GetAsync(url);
            string xml= await respuesta.Content.ReadAsStringAsync();

            List<EstadoCuentaViewModel> model=ParsearEstadoCuentaXml(xml);
            return View(model);
        }

        private List<EstadoCuentaViewModel> ParsearEstadoCuentaXml(string xml)
        {
            XmlDocument doc =new XmlDocument();
            doc.LoadXml(xml);
            var resultado=new List<EstadoCuentaViewModel>();

            XmlNodeList clientesNodes= doc.SelectNodes("/estadosCuenta/cliente");
            if (clientesNodes ==null || clientesNodes.Count== 0)
                clientesNodes= doc.SelectNodes("/cliente");

            if (clientesNodes!= null)
            {
                foreach (XmlNode nodoCliente in clientesNodes)
                {
                    EstadoCuentaViewModel EstadoCuentaVm=new EstadoCuentaViewModel
                    {
                        Nit =nodoCliente.Attributes["nit"]?.Value,
                        Nombre= nodoCliente.Attributes["nombre"]?.Value,
                        SaldoActual=decimal.Parse(nodoCliente.Attributes["saldoActual"]?.Value ?? "0"),
                        Transacciones =new List<TransaccionViewModel>()
                    };

                    XmlNodeList transNodes=nodoCliente.SelectNodes("transacciones/transaccion");
                    if (transNodes!= null)
                    {
                        foreach (XmlNode t in transNodes)
                        {
                            EstadoCuentaVm.Transacciones.Add(new TransaccionViewModel
                            {
                                Fecha=DateTime.Parse(t.Attributes["fecha"]?.Value),
                                Cargo =decimal.Parse(t.Attributes["cargo"]?.Value ?? "0"),
                                Abono= decimal.Parse(t.Attributes["abono"]?.Value ?? "0"),
                                Descripcion = t.Attributes["descripcion"]?.Value
                            });
                        }
                    }
                    resultado.Add(EstadoCuentaVm);
                }
            }
            return resultado;
        }


        // GET: Ingresos
        public async Task<IActionResult> Ingresos(int? mes, int? anio)
        {
            if (!mes.HasValue || !anio.HasValue)
                return View(new IngresosViewModel());

            HttpClient cliente=_httpClientFactory.CreateClient("ClienteAPI");
            string url =$"/api/Consultas/ingresos?mes={mes}&anio={anio}";
            HttpResponseMessage respuesta= await cliente.GetAsync(url);
            string xml=await respuesta.Content.ReadAsStringAsync();

            IngresosViewModel model =ParsearIngresosXml(xml);
            model.MesSeleccionado=new DateTime(anio.Value, mes.Value, 1);
            return View(model);
        }

        private IngresosViewModel ParsearIngresosXml(string xml)
        {
            XmlDocument doc =new XmlDocument();
            doc.LoadXml(xml);
            IngresosViewModel model= new IngresosViewModel { Meses = new List<MesIngreso>() };

            XmlNodeList nodos = doc.SelectNodes("/ingresos/mes");
            if (nodos != null)
            {
                foreach (XmlNode nodo in nodos)
                {
                    model.Meses.Add(new MesIngreso{Nombre=nodo.Attributes["nombre"]?.Value, Total=decimal.Parse(nodo.Attributes["total"]?.Value ?? "0")});
                }
            }
            return model;
        }

        public IActionResult Ayuda()
        {
            return View();
        }

    }

}


