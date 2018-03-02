using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Component
{
    public interface IValueConverter
    {
        IValue Convert(IValue old);
        IValue ConvertBack(IValue old);
    }
}
