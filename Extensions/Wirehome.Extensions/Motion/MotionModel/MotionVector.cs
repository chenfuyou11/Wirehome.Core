using System;
using System.Collections.Generic;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionVector : IEquatable<MotionVector>
    {
        public MotionPoint Start { get; }
        public MotionPoint End { get; }
        public List<MotionPoint> Confiusions { get; private set; } = new List<MotionPoint>();
        

        public MotionVector() { }
        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            Start = startPoint;
            End = endPoint;
        }
        
        public bool Contains(MotionPoint p) => Start.Equals(p) || End.Equals(p);

        /// <summary>
        /// If there is move in neighborhood of END point other then START it means thet vector couldbe not real becouse move can be from other staring point
        /// </summary>
        /// <param name="confiusions"></param>
        public MotionVector RegisterMotionConfusions(IEnumerable<MotionPoint> confiusions)
        {
            Confiusions.AddRange(confiusions);
            return this;
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
