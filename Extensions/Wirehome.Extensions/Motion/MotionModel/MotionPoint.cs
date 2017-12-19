using System;
using Wirehome.Contracts.Sensors;


namespace Wirehome.Extensions.MotionModel
{
    public class MotionPoint : IEquatable<MotionPoint>
    {
        public string Uid { get; }
        public DateTimeOffset TimeStamp { get; }

        public MotionPoint(string place, DateTimeOffset time)
        {
            Uid = place;
            TimeStamp = time;
        }

        public override string ToString()
        {
            return $"[{Uid}: {TimeStamp:ss:fff}]";
        }

        private bool IsEqual(MotionPoint other)
        {
            return Equals(Uid, other.Uid);
        }

        public bool Equals(MotionPoint other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsEqual(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return IsEqual((MotionPoint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Uid?.GetHashCode() ?? 0) * 397) ^ TimeStamp.GetHashCode();
            }
        }

        public static bool operator ==(MotionPoint motionA, MotionPoint monionB)
        {
            return ReferenceEquals(motionA, monionB) || motionA.Equals(monionB);
        }

        public static bool operator !=(MotionPoint motionA, MotionPoint motionB)
        {
            return !(motionA == motionB);
        }
    }
}
