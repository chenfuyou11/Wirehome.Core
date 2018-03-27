namespace Wirehome.ComponentModel.Commands
{
    public class Command : BaseObject
    {
        public static Command RefreshCommand = new Command(CommandType.RefreshCommand);

        public Command()
        {
            SupressPropertyChangeEvent = true;
        }

        public Command(string commandType) : base()
        {
            Type = commandType;
        }
    }
}