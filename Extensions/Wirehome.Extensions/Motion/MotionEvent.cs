namespace Wirehome.Extensions
{
    public class MotionEvent
    {
        public MotionEvent(string uid)
        {
            MotionDetectorUID = uid;
        }

        public string MotionDetectorUID { get; }
    }
}
