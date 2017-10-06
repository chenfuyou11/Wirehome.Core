using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class DisplayController : Controller
    {
        //[HttpGet]
        //public bool Get()
        //{
        //    return true;
        //}

        [HttpPost]
        public IActionResult Post(DisplayMode displayMode)
        {
            DisplayService.SetDisplayMode(displayMode);

            return Ok();
        }
    }
}
