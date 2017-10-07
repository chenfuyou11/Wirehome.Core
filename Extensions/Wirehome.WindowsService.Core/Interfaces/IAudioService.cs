using Wirehome.WindowsService.Audio;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Core
{
    public interface IAudioService
    {
        bool? GetApplicationMute(int pid);
        float? GetApplicationVolume(int pid);
        AudioDeviceCollection GetAudioDevices(AudioDeviceKind kind, AudioDeviceState state);
        AudioDevice GetDefaultAudioDevice(AudioDeviceKind kind, AudioDeviceRole role);
        AudioDevice GetDevice(string id);
        float GetMasterVolume();
        bool GetMasterVolumeMute();
        bool IsDefaultAudioDevice(AudioDevice device, AudioDeviceRole role);
        void SetApplicationMute(int pid, bool mute);
        void SetApplicationVolume(int pid, float level);
        void SetDefaultAudioDevice(AudioDevice device);
        void SetDefaultAudioDevice(AudioDevice device, AudioDeviceRole role);
        void SetMasterVolume(float newLevel);
        void SetMasterVolumeMute(bool isMuted);
        float StepMasterVolume(float stepAmount);
        bool ToggleMasterVolumeMute();
    }
}