using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections;

namespace Wirehome.Extensions
{
    public class ImmutableLazyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private ImmutableDictionary<TKey, TValue>.Builder _builder;
        private ImmutableDictionary<TKey, TValue> _innerCollection;
        private bool _IsInitialized;

        public ImmutableLazyDictionary()
        {
            _builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();
        }

        public void Initialize()
        {
            _IsInitialized = true;
            _innerCollection = _builder.ToImmutable();
        }

        public void Add(TKey key, TValue value)
        {
            if (_IsInitialized) throw new Exception("Cannot add new elements after initialization");

            _builder.Add(key, value);
        }

        public void ForEach(Action<TValue> action)
        {
            foreach (TValue item in _innerCollection.Values) action(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ChackAccess();
            return _innerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            ChackAccess();
            return _innerCollection.GetEnumerator();
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                ChackAccess();
                return _innerCollection.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                ChackAccess();
                return _innerCollection.Values;
            }
        }

        private void ChackAccess()
        {
            if (!_IsInitialized) throw new Exception("Cannot access before class isInitialized");
        }

        public TValue this[TKey key]
        {
            set
            {
                if (_IsInitialized) throw new Exception("Cannot add new elements after initialization");
                _builder[key] = value;
            }
            get
            {
                if (_IsInitialized)
                {
                    return _innerCollection[key];
                }
                else
                {
                    return _builder[key];
                }
            }
        }

        public int Count
        {
            get
            {
                ChackAccess();
                return _innerCollection.Count;

            }
        }
    }
}