using System;
using System.Collections.Generic;
using System.Text;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.Core.Extensions
{
    public static class IValueExtensions
    {
        public static DoubleValue ToDoubleValue(this IValue value) => (value as DoubleValue)?.Value;
    }
}