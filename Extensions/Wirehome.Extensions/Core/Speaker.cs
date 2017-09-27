using Wirehome.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Components.Features;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Contracts;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions.Core
{
    public class Speaker : ComponentBase
    {
        private SortedList<Enum, string> _Sounds;
        private SpeakerStateValue _speakerState = SpeakerStateValue.Stopped;
        private CommandExecutor _commandExecutor;
        private Random _soundIndexGenerator;
        private string _nextSound;
        private object _syncRoot = new object();
        private readonly INativeSoundPlayer _soundPlayer;

        public Speaker(string id, Dictionary<Enum, string> sounds, INativeSoundPlayer soundPlayer) : base(id)
        {
            _Sounds = new SortedList<Enum, string>(sounds);

            _commandExecutor = new CommandExecutor();
            _commandExecutor.Register<TurnOnCommand>(c => PlayRandom());
            _commandExecutor.Register<PlayCommand>(c => PlaySpecific(c));
            _commandExecutor.Register<TurnOffCommand>(c => Stop());

            _soundIndexGenerator = new Random();
            this._soundPlayer = soundPlayer ?? throw new ArgumentNullException(nameof(soundPlayer));
            this._soundPlayer.SoundEnd = () =>
            {
                SetInternalState(SpeakerStateValue.Stopped, true);
            };
        }
        
        private void SetInternalState(SpeakerStateValue value, bool skipPlayerManipulation = false)
        {
            lock (_syncRoot)
            {
                var oldState = GetState();

                if (!skipPlayerManipulation)
                {
                    if (value == SpeakerStateValue.Stopped)
                    {
                        _soundPlayer.Pause();
                    }
                    else if (value == SpeakerStateValue.Playing)
                    {
                        //TODO check for exist

                        // Add avability to queue

                        if (_speakerState == SpeakerStateValue.Playing)
                        {
                            return;
                        }

                        _soundPlayer.Play(_nextSound);
                    }
                }

                _speakerState = value;

                OnStateChanged(oldState);
            }
        }

        public void PlayRandom()
        {
            var sound = _Sounds.ElementAt(_soundIndexGenerator.Next(0, _Sounds.Count - 1)).Value;
            Play(sound);
        }

        public void PlaySpecific(PlayCommand command)
        {
            if(!_Sounds.ContainsKey(command.Sound))
            {
                throw new Exception($"Sound {command.Sound} is not registred in Speaker component");
            }

            _nextSound = _Sounds[command.Sound];
            SetInternalState(SpeakerStateValue.Playing);
        }

        public void Stop()
        {
            SetInternalState(SpeakerStateValue.Stopped);
        }

        public void Play(string sound)
        {
            _nextSound = sound;
            SetInternalState(SpeakerStateValue.Playing);   
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection().With(new PowerStateFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection().With(new SpeakerState(_speakerState));
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }
    }

    public class SpeakerState : EnumBasedState<SpeakerStateValue>
    {
        public static readonly SpeakerState Playing = new SpeakerState(SpeakerStateValue.Playing);
        public static readonly SpeakerState Stopped = new SpeakerState(SpeakerStateValue.Stopped);

        public SpeakerState(SpeakerStateValue value) : base(value)
        {
        }
    }

    public enum SpeakerStateValue
    {
        Playing,
        Stopped
    }

    public class PlayCommand : ICommand
    {
        public Enum Sound { get; set; }

        public PlayCommand(Enum sound)
        {
            Sound = sound;
        }
    }

    


}
