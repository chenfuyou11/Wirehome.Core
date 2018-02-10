using Windows.Media.Playback;
using Windows.Media.Core;
using Wirehome.Contracts.Core;
using System;

namespace Wirehome.Raspberry.Core
{
    public class RaspberrySoundPlayer : INativeSoundPlayer
    {
        private readonly MediaPlayer _player;
        public Action SoundEnd { get; set; }

        public RaspberrySoundPlayer()
        {
            _player = new MediaPlayer()
            {
                AutoPlay = false
            };

            _player.MediaEnded += _player_MediaEnded;
        }

        private void _player_MediaEnded(MediaPlayer sender, object args) => SoundEnd?.Invoke();

        public void Pause() => _player.Pause();

        public void Play(string sound)
        {
            _player.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///{sound}"));
            _player.Play();
        }

        
    }
}
