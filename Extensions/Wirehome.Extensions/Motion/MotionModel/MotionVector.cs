using CSharpFunctionalExtensions;
using System;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionVector : ValueObject<MotionVector>, IEquatable<MotionVector>
    {
        public MotionPoint Start { get; }
        public MotionPoint End { get; }

        public MotionVector() { }
        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            Start = startPoint;
            End = endPoint;
        }
        
        public bool Contains(MotionPoint p) => Start.Equals(p) || End.Equals(p);
        public override string ToString() =>  $"{Start} -> {End}";
        protected override bool EqualsCore(MotionVector other) => other.Start.Equals(Start) && other.End.Equals(End);
        public bool Equals(MotionVector other) => base.Equals(other);
        
        protected override int GetHashCodeCore()
        {
            unchecked
            {
                return ((Start?.GetHashCode() ?? 0) * 397) ^ End.GetHashCode();
            }
        }
    }
}
