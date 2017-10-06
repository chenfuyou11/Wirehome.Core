using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class VolumeController : Controller
    {
        [HttpGet]
        public float Get()
        {
            return AudioService.GetMasterVolume();
        }
        
        [HttpPost]
        public IActionResult Post(float value)
        {
            AudioService.SetMasterVolume(value);

            return Ok();
        }
    }
}
