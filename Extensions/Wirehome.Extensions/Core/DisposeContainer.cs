using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Extensions.Core
{
    public class DisposeContainer : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly object _disposeLock = new object();
        private volatile bool _disposed;

        public DisposeContainer(params IDisposable[] disposables)
        {
            _disposables.AddRange(disposables);
        }
        
        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void Add(params IDisposable[] disposables)
        {
            _disposables.AddRange(disposables);
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (_disposeLock)
            {
                if (_disposed) return;
                foreach (var d in _disposables)
                {
                    d.Dispose();
                }
                _disposables.Clear();
                _disposed = true;
            }
        }
    }
}
