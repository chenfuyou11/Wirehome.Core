using System;
using System.Collections.Generic;
using System.Text;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.Core.Extensions
{
    public static class IValueExtensions
    {
        public static DoubleValue ToDoubleValue(this IValue value) => (value as DoubleValue)?.Value;

        public static StringValue ToStringValue(this IValue value) => (value as StringValue)?.Value;

        public static TimeSpan ToTimeSpan(this IValue value) => TimeSpan.FromMilliseconds((double)(value as IntValue)?.Value);
    }
}