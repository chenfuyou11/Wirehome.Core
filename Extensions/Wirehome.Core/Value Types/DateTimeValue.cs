using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace Wirehome.Core
{
    public class DateTimeValue : ValueObject, IValue
    {
        public DateTimeValue(DateTimeOffset value) => Value = value;

        public DateTimeOffset Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator DateTimeValue(DateTimeOffset value) => new DateTimeValue(value);
        public static implicit operator DateTimeOffset(DateTimeValue value) => value.Value;
    }    
}
