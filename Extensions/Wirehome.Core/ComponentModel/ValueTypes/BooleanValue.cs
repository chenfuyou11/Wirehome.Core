using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Wirehome.ComponentModel.ValueTypes
{
    public class BooleanValue : ValueObject, IValue
    {
        public BooleanValue(bool value) => Value = value;

        public bool Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
