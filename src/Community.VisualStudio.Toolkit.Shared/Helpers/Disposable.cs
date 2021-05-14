using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An <c>IDisposable</c> implementation that calls a delegate when disposed. 
    /// </summary>
    public sealed class Disposable : IDisposable
    {
        Action? _onDispose;

        ///<summary>Creates a non-repeatable Disposable instance.</summary>
        ///<param name="disposer">The delegate to be called by the Dispose method.  The delegate will only be called once.</param>
        public Disposable(Action disposer) : this(disposer, false) { }

        ///<summary>Creates a Disposable instance.</summary>
        ///<param name="disposer">The delegate to be called by the Dispose method.</param>
        ///<param name="repeatable">Indicates whether the underlying delegate should be called multiple times if this instance is disposed multiple times.</param>
        public Disposable(Action disposer, bool repeatable)
        {
            _onDispose = disposer ?? throw new ArgumentNullException("disposer");
            Repeatable = repeatable;
        }

        ///<summary>Gets whether the underlying delegate will be called multiple times if this instance is disposed multiple times.</summary>
        public bool Repeatable { get; private set; }

        ///<summary>Gets whether the Dispose method has been called.</summary>
        public bool Disposed { get; private set; }

        ///<summary>Calls the disposer delegate specified in the constructor.</summary>
        public void Dispose()
        {
            if (Disposed && !Repeatable)
            {
                return;
            }

            Disposed = true;
            _onDispose?.Invoke();

            if (!Repeatable)
            {
                _onDispose = null;  //Free the reference to allow the delegate & its Target to be GC'd.
            }
        }
    }
}
