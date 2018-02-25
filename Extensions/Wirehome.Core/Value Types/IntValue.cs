using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Wirehome.Core
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


    }    
}
