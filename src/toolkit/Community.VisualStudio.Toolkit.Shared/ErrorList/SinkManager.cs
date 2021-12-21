using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// ITableDataSink wrapper which manages and notifies of error snapshots
    /// </summary>
    internal class SinkManager : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Underlying sink
        /// </summary>
        private readonly ITableDataSink _sink;

        /// <summary>
        /// Dispose action
        /// </summary>
        private readonly Action<SinkManager> _onDispose;

        /// <summary>
        /// Snapshot collection
        /// </summary>
        private readonly List<TableEntriesSnapshot> _snapshots = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sink">Sink to be wrapped</param>
        /// <param name="onDispose">Dispose action to be done on token disposal</param>
        public SinkManager(ITableDataSink sink, Action<SinkManager> onDispose)
        {
            _sink = sink;
            _onDispose = onDispose;
        }

        /// <summary>
        /// Notifies the underlying sink of the new errors
        /// </summary>
        /// <param name="snapshots"></param>
        public void UpdateSink(IEnumerable<TableEntriesSnapshot> snapshots)
        {
            foreach (TableEntriesSnapshot snapshot in snapshots)
            {
                TableEntriesSnapshot existing = _snapshots.FirstOrDefault(s => s.FilePath == snapshot.FilePath);

                if (existing != null)
                {
                    _snapshots.Remove(existing);
                    _sink.ReplaceSnapshot(existing, snapshot);
                }
                else
                {
                    _sink.AddSnapshot(snapshot);
                }

                _snapshots.Add(snapshot);
            }
        }

        /// <summary>
        /// Removes all registered errors
        /// </summary>
        public void Clear()
        {
            _sink.RemoveAllSnapshots();
            _snapshots.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _onDispose?.Invoke(this);
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
