using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.Core
{
    public class DoubleValue : ValueObject, IValue
    {
        public DoubleValue(double value) => Value = value;

        public double Value
        {
            get;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }


    }    
}
