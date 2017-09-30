using System;
using System.Collections.Generic;

namespace Wirehome.Extensions.Extensions
{

    public class EqualityDecorator<T> : IEquatable<EqualityDecorator<T>>
    {
        private readonly T item;
        private DateTime created;

        public EqualityDecorator(T item)
        {
            this.item = item;
            this.created = DateTime.UtcNow;
        }

        public T Item
        {
            get { return item; }
        }

        public DateTime Created
        {
            get { return created; }
        }

        public bool Equals(EqualityDecorator<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Item, other.Item);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EqualityDecorator<T>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Item);
        }

        public static bool operator ==(EqualityDecorator<T> left, EqualityDecorator<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EqualityDecorator<T> left, EqualityDecorator<T> right)
        {
            return !Equals(left, right);
        }
    }
}
