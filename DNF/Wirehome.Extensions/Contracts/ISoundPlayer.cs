using System;
using System.Collections.Generic;
using System.Text;

namespace HA4IoT.Extensions.Contracts
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
