using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel
{
    public sealed class Property : ValueObject
    {
        public string Type { get; set; }
        public IValue Value { get; set; }
        public override string ToString() => $"{Type}={Convert.ToString(Value)}";
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
        }
    }
}
