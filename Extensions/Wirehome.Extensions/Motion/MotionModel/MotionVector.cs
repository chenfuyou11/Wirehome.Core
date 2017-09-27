using Wirehome.Extensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionVector : IEquatable<MotionVector>
    {
        public MotionPoint Start => Path.FirstOrDefault();

        public MotionPoint End => Path.LastOrDefault();

        public List<MotionPoint> Path { get; } = new List<MotionPoint>();
        
        public override string ToString()
        {
            return string.Join("->", Path.Select(x => x.MotionDetector.Id));
        }

        public bool Equals(MotionVector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqual(other);
        }

        private bool IsEqual(MotionVector other)
        {
            return Path?.SequenceEqual(other?.Path) ?? false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return IsEqual((MotionVector) obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCodeOfElements();
        }
    }
}
