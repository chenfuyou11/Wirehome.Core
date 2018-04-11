namespace Wirehome.Contracts.Components.States
{
    public class LevelState : IComponentFeatureState
    {
        public LevelState(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}
