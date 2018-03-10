using System;

namespace Wirehome.Core.Native
{
    public interface INativeSoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
