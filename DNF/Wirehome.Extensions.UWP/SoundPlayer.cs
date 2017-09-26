using Wirehome.Extensions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.Core;

namespace Wirehome.Extensions.UWP
{
    public class SoundPlayer : ISoundPlayer
    {
        MediaPlayer _player;
        public Action SoundEnd { get; set; }

        public SoundPlayer()
        {
            _player = new MediaPlayer()
            {
                AutoPlay = false
            };

            _player.MediaEnded += _player_MediaEnded;
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Play(string sound)
        {
            _player.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///{sound}"));
            _player.Play();
        }

        private void _player_MediaEnded(MediaPlayer sender, object args)
        {
            SoundEnd?.Invoke();
        }
    }
}
