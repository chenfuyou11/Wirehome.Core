﻿using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.ComponentModel.ValueTypes
{
    public class StringListValue : ValueObject, IValue
    {
        public StringListValue(params string[] value) => Value = value;

        public string[] Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => string.Join(", ", Value);
    }
}
