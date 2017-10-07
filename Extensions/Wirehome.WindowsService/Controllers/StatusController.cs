using Microsoft.AspNetCore.Mvc;
using Wirehome.Extensions.Devices;
using Wirehome.Extensions.Devices.Computer;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private readonly IAudioService _audioService;

        public StatusController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public ComputerStatus Get()
        {
            return new ComputerStatus
            {
                MasterVolume = _audioService.GetMasterVolume(),
                Mute = _audioService.GetMasterVolumeMute(),
                PowerStatus = Contracts.Components.States.PowerStateValue.On,
                ActiveInput = _audioService.GetDefaultAudioDevice().ToString()
            };
        }
    }

   
}
