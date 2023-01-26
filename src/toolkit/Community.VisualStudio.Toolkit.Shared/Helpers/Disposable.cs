using System;

namespace Community.VisualStudio.Toolkit
{
    internal sealed class Disposable : IDisposable
    {
        private readonly Action _action;
        private bool _disposed;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _action();
            }
        }
    }
}
