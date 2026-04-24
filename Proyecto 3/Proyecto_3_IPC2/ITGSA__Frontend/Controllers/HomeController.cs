using ITGSA__Frontend.Models;
using Microsoft.AspNetCore.Mvc;
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

                HttpResponseMessage respuesta = await cliente.PostAsync("/api/Transacciones/cargar", contenido);
                string xmlRespuesta =await respuesta.Content.ReadAsStringAsync();
                ViewBag.RespuestaXml=xmlRespuesta;
            }
            return View();
        }


        public IActionResult CargarConfiguracion()
        {
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
                ViewBag.RespuestaXml = xmlRespuesta;
            }

            return View();
        }
    }
}
