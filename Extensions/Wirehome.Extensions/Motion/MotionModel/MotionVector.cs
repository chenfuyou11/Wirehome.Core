using System;
using System.Collections.Generic;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionVector : IEquatable<MotionVector>
    {
        public MotionPoint Start { get; }
        public MotionPoint End { get; private set;}
        public List<MotionPoint> Confiusions { get; private set; } = new List<MotionPoint>();
        

        public MotionVector() { }
        public MotionVector(MotionPoint startPoint)
        {
            Start = startPoint;
        }
        
        public bool Contains(MotionPoint p)
        {
            return Start.Equals(p)|| End.Equals(p);
        }

        public bool IsComplete()
        {
            return End != null;
        }

        public void SetEnd(MotionPoint end)
        {
            End = end;
        }

        public void RegisterMotionConfusions(List<MotionPoint> confiusions)
        {
            Confiusions.AddRange(confiusions);
        }

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }

        public bool Equals(MotionVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqual(other);
        }

        private bool IsEqual(MotionVector other)
        {
            return other.Start.Equals(this.Start)
                   && other.End.Equals(this.End);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return IsEqual((MotionVector) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Start?.GetHashCode() ?? 0) * 397) ^ End.GetHashCode();
            }
        }
    }
}
