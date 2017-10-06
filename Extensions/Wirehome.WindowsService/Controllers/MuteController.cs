using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class MuteController : Controller
    {
        [HttpGet]
        public bool Get()
        {
            return AudioService.GetMasterVolumeMute();
        }
        
        [HttpPost]
        public IActionResult Post(bool value)
        {
            AudioService.SetMasterVolumeMute(value);

            return Ok();
        }
    }
}
