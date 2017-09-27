using System;

namespace Wirehome.Contracts.Core
{
    public interface INativeSoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
