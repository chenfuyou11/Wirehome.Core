using System;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        string Id { get; }

        IComponentFeatureStateCollection GetState();

        IComponentFeatureCollection GetFeatures();

        void ExecuteCommand(ICommand command);
    }
}
