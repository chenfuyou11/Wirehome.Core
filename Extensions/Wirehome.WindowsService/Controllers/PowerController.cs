using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Services;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class PowerController : Controller
    {
        [HttpGet]
        public bool Get()
        {
            return true;
        }
        
        [HttpPost]
        public IActionResult Post(PowerState value)
        {
            PowerService.SetPowerMode(value);
            return Ok();
        }
    }
}
