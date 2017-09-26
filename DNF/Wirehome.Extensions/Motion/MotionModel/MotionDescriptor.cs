using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Components;
using HA4IoT.Extensions.Core;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Conditions;
using System.Linq;
using HA4IoT.Contracts.Components.Commands;

namespace HA4IoT.Extensions.MotionModel
{
    public class MotionDescriptor
    {
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly object _syncRoot = new object();
        //private bool _isInitialized = false;

        public IMotionDetector MotionDetector { get; private set; }

        public IMotionDetector[] Neighbors { get; private set; }

        public IComponent Lamp { get; private set; }
        
        public IArea Area { get; private set; }

        public IObservable<IMotionDetector> MotionSource { get; private set; }


        public WriteOnce<AreaType> AreaType { get; set; } = new WriteOnce<AreaType>(MotionModel.AreaType.Room);

        public WriteOnce<bool> DisableAtNight { get; set; } = new WriteOnce<bool>(true);

        public WriteOnce<bool> DisableAtDayLight { get; set; } = new WriteOnce<bool>(true);

        public WriteOnce<bool> DisableLamptAutomation { get; set; } = new WriteOnce<bool>(false);

        public WriteOnce<int> MaxPersonCapacity { get; set; } = new WriteOnce<int>(10);

        public WriteOnce<TimeSpan> TimeToLive { get; set; } = new WriteOnce<TimeSpan>(TimeSpan.FromSeconds(60));

        public WriteOnce<double> PresenceProbability { get; private set; } = new WriteOnce<double>(0.0);


        // TODO Collision resolution source
        // TODO maybe statistics for some period?
        public DateTimeOffset LastMotionTime { get; private set; }

        public double CurrentPresenceProbability { get; private set; }

        public TimeSpan CurrentTimeToLive { get; private set; }



        public MotionDescriptor(IMotionDetector motionDetector, IMotionDetector[] neighbors, IComponent lamp)
        {
            MotionDetector = motionDetector;
            Neighbors = neighbors;
            Lamp = lamp;

            MotionSource = MotionDetector.ToObservable();
        }

        public void InitDescriptor()
        {
            //_isInitialized = true;
        }

        public void SetArea(IArea area)
        {
            if (Area != null)
            {
                throw new Exception("Area could be initaized onlny once");
            }

            Area = area ?? throw new Exception($"Motion detector {MotionDetector.Id} was not register in any area");
        }

        public void SetLastMotionTime(DateTimeOffset time)
        {
            LastMotionTime = time;
        }

        public void TryTurnOnLamp()
        {
            lock (_syncRoot)
            {
                //TODO ustawiać to w jednym miejscu
                CurrentPresenceProbability = 1.0;

                if (CanTurnOnLamp())
                {
                    Lamp.ExecuteCommand(new TurnOnCommand());
                }
            }
        }

        public void TryTurnOffLamp()
        {
            lock (_syncRoot)
            {
                //TODO ustawiać to w jednym miejscu
                CurrentPresenceProbability = 0.0;

                if (CanTurnOffLamp())
                {
                    Lamp.ExecuteCommand(new TurnOffCommand());
                }
            }
        }

        public bool CanTurnOnLamp()
        {
            if (_disablingConditionsValidator.Conditions.Any() && _disablingConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            if (_enablingConditionsValidator.Conditions.Any() && _enablingConditionsValidator.Validate() == ConditionState.NotFulfilled)
            {
                return false;
            }

            return true;
        }

        public bool CanTurnOffLamp()
        {
            if (_turnOffConditionsValidator.Conditions.Any() && _turnOffConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            return true;
        }
    }
}


