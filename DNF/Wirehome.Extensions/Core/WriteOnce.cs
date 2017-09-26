using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Core
{
    public sealed class WriteOnce<T>
    {
        private T value;
        private bool hasValue;
        private bool hasDefaultValue;


        public WriteOnce()
        {
            hasDefaultValue = false;
        }

        public WriteOnce(T val)
        {
            value = val;
            hasDefaultValue = true;
        }

        public override string ToString()
        {
            return hasValue ? Convert.ToString(value) : "";
        }
        public T Value
        {
            get
            {
                if (!hasValue && !hasDefaultValue) throw new InvalidOperationException("Value not set");
                return value;
            }
            set
            {
                if (hasValue) throw new InvalidOperationException("Value already set");
                this.value = value;
                this.hasValue = true;
            }
        }
        public T ValueOrDefault { get { return value; } }

        public static implicit operator T(WriteOnce<T> value) { return value.Value; }
    }
}
