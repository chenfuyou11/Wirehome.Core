using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.Core
{
    public class StringValue : ValueObject, IValue
    {
        public StringValue(string value) => Value = value;

        public string Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator StringValue(string value) => new StringValue(value);
        public static implicit operator string(StringValue value) => value.Value;
    }
}
