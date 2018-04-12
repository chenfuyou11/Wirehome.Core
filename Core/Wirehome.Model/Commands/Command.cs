using System.Threading;

namespace Wirehome.ComponentModel.Commands
{
    public class Command : BaseObject
    {
        public static Command RefreshCommand = new Command(CommandType.RefreshCommand);
        public static Command TurnOnCommand = new Command(CommandType.TurnOnCommand);
        public static Command TurnOffCommand = new Command(CommandType.TurnOffCommand);
        public static Command DiscoverCapabilitiesCommand = new Command(CommandType.DiscoverCapabilities);

        public CancellationToken CancellationToken { get; }

        public Command()
        {
            SupressPropertyChangeEvent = true;
        }

        public Command(string commandType) : base()
        {
            Type = commandType;
        }

        public Command(string commandType, CancellationToken cancellationToken) : base()
        {
            Type = commandType;
            CancellationToken = cancellationToken;
        }

        public static implicit operator Command(string value) => new Command(value);
    }
}