using System.Collections.Generic;
using Wirehome.Extensions.MotionModel;

namespace Wirehome.Extensions
{
    public class ConfusedVector
    {
        private readonly List<MotionPoint> _confusionPoints;

        public ConfusedVector(MotionVector vector, IEnumerable<MotionPoint> confusionPoints)
        {
            Vector = vector;
            _confusionPoints = new List<MotionPoint>(confusionPoints);
        }

        public ConfusedVector ReduceConfusion(MotionPoint confusionPoint)
        {
            _confusionPoints.Remove(confusionPoint);
            return this;
        }

        public bool IsConfused => _confusionPoints.Count > 0;

        public MotionVector Vector { get; }
        public IReadOnlyCollection<MotionPoint> ConfusionPoint => _confusionPoints.AsReadOnly();
    }
}