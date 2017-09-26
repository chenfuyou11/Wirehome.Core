namespace Wirehome.Contracts.Components.Features
{
    public class LevelFeature : IComponentFeature
    {
        public int MaxLevel { get; set; }

        public int DefaultActiveLevel { get; set; }
    }
}
