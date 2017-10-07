using Microsoft.AspNetCore.Mvc;
using Wirehome.Extensions.Devices.Computer;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class VolumeController : Controller
    {
        private readonly IAudioService _audioService;

        public VolumeController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public float Get()
        {
            return _audioService.GetMasterVolume();
        }
        
        [HttpPost]
        public IActionResult Post([FromBody] VolumePost volume)
        {
            _audioService.SetMasterVolume(volume.Volume);
            return Ok();
        }
    }

   
}
