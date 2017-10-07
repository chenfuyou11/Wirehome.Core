using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Core;
using System.Linq;
using System.Collections.Generic;
using Wirehome.Extensions.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class InputSourceController : Controller
    {
        private readonly IAudioService _audioService;

        public InputSourceController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public IEnumerable<AudioDeviceInfo> Get()
        {
            var devices = _audioService.GetAudioDevices(Interop.AudioDeviceKind.Playback, Interop.AudioDeviceState.Active);
            return devices.Select(x => new AudioDeviceInfo { Id = x.Id, Name = x.ToString() }).ToList();
        }

        [HttpPost]
        public IActionResult Post(string id)
        {
            var device = _audioService.GetAudioDevices(Interop.AudioDeviceKind.Playback, Interop.AudioDeviceState.Active).FirstOrDefault(x => x.Id == id);
            if (device == null) throw new System.Exception($"Device {id} was not found");
            
            _audioService.SetDefaultAudioDevice(device);
            return Ok();
        }

    }
}
