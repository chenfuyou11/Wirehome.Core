using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.ComponentModel.ValueTypes
{
    public class IntValue : ValueObject, IValue
    {
        public IntValue(int value) => Value = value;

        public int Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator IntValue(int value) => new IntValue(value);
        public static implicit operator int(IntValue value) => value.Value;

    }    
}
