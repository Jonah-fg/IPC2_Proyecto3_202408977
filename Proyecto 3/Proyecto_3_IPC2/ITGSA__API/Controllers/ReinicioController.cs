using ITGSA.API.Services;
using ITGSA__API.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITGSA__API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReinicioController : ControllerBase
    {
        private readonly AlmacenamientoXml _almacenamiento;

        public ReinicioController(AlmacenamientoXml almacenamiento)
        {
            _almacenamiento=almacenamiento;
        }

        [HttpPost]
        public IActionResult Reiniciar()
        {
            _almacenamiento.ResetearDatos();
            string xmlRespuesta ="<mensaje>Datos reiniciados correctamente</mensaje>";
            return Content(xmlRespuesta, "application/xml");
        }
    }
}
    
