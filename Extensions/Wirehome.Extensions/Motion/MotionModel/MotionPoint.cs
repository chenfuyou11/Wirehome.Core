using System;
using Wirehome.Contracts.Sensors;


namespace Wirehome.Extensions.MotionModel
{
    public class MotionPoint : IEquatable<MotionPoint>
    {
        public IMotionDetector MotionDetector { get; }

        public DateTimeOffset TimeStamp { get; }

        public MotionPoint(IMotionDetector place, DateTimeOffset time)
        {
            MotionDetector = place;
            TimeStamp = time;
        }

        private bool IsEqual(MotionPoint other)
        {
            return Equals(MotionDetector.Id, other.MotionDetector.Id);
                //&& TimeStamp.Equals(other.TimeStamp);
        }

        public bool Equals(MotionPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEqual(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return IsEqual((MotionPoint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((MotionDetector?.GetHashCode() ?? 0) * 397) ^ TimeStamp.GetHashCode();
            }
        }

        public static bool operator ==(MotionPoint motionA, MotionPoint monionB)
        {
            return (ReferenceEquals(motionA, monionB) || motionA.Equals(monionB));
        }

        public static bool operator !=(MotionPoint motionA, MotionPoint motionB)
        {
            return !(motionA == motionB);
        }
    }
}
