using Wirehome.Extensions.MotionModel;

namespace Wirehome.Extensions
{
    public class Motion
    {
        public Motion(MotionPoint start)
        {
            Start = start;
        }

        public MotionPoint Start { get; }
        public MotionVector Vector { get; private set; }

        public void CreateVector(MotionPoint mp)
        {
            Vector = new MotionVector(Start, mp);
        }

        public override string ToString()
        {
            return $"{Start} || {Vector?.ToString() ?? "<>"}";
        }
    }
}