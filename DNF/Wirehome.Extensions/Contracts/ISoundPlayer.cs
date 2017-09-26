using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Extensions.Contracts
{
    public interface ISoundPlayer
    {
        Action SoundEnd { get; set; }
        void Pause();
        void Play(string sound);
    }
}
