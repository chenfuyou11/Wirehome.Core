using System;

namespace Wirehome.Extensions.MotionModel
{
    public interface IMotionConfigurationProvider
    {
        MotionConfiguration GetConfiguration();
    }
}