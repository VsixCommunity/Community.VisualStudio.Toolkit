using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A data source for the Error List.
    /// </summary>
    public class TableDataSource : ITableDataSource
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="name">A unique string. It's often the name of the extension itself.</param>
        public TableDataSource(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Identifier = name;
            DisplayName = name;
            Initialize();
        }

        /// <summary>
        /// Data sink subscriptions
        /// </summary>
        private readonly List<SinkManager> _managers = new();

        /// <summary>
        /// Error list snapshots
        /// </summary>
        private readonly Dictionary<string, TableEntriesSnapshot> _snapshots = new();

        /// <summary>
        /// 'Error list' columns/components exposed by items managed by this data source
        /// </summary>
        public virtual IReadOnlyCollection<string> Columns { get; } = new[]
        {
        StandardTableColumnDefinitions.DetailsExpander,
        StandardTableColumnDefinitions.ErrorCategory,
        StandardTableColumnDefinitions.ErrorSeverity,
        StandardTableColumnDefinitions.ErrorCode,
        StandardTableColumnDefinitions.ErrorSource,
        StandardTableColumnDefinitions.BuildTool,
        StandardTableColumnDefinitions.Text,
        StandardTableColumnDefinitions.DocumentName,
        StandardTableColumnDefinitions.Line,
        StandardTableColumnDefinitions.Column
    };

        /// <inheritdoc/>
        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        /// <inheritdoc/>
        public virtual string Identifier { get; }

        /// <inheritdoc/>
        public virtual string DisplayName { get; }

        /// <inheritdoc/>
        public IDisposable Subscribe(ITableDataSink sink)
        {
            SinkManager? manager = new(sink, RemoveSinkManager);

            AddSinkManager(manager);

            return manager;
        }

        /// <summary>
        /// Initializes the table manager.
        /// </summary>
        protected void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ITableManagerProvider tableManagerProvider = VS.GetMefService<ITableManagerProvider>();
            Assumes.Present(tableManagerProvider);

            ITableManager manager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(this, Columns);
        }

        /// <summary>
        /// Registers a sink subscription
        /// </summary>
        /// <param name="manager">Subscription to register</param>
        private void AddSinkManager(SinkManager manager)
        {
            lock (_managers)
            {
                _managers.Add(manager);
            }
        }

        /// <summary>
        /// Unregisters a previously registered sink subscription
        /// </summary>
        /// <param name="manager">Subscription to unregister</param>
        private void RemoveSinkManager(SinkManager manager)
        {
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        /// <summary>
        /// Notifies all subscribers of an update in error (listings)
        /// </summary>
        private void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (SinkManager manager in _managers)
                {
                    manager.UpdateSink(_snapshots.Values);
                }
            }
        }

        /// <summary>
        /// Adds errors to the applicable snapshots.
        /// </summary>
        public void AddErrors(IEnumerable<ErrorListItem> errors)
        {
            if (errors == null || !errors.Any())
            {
                return;
            }

            string? projectName = errors.FirstOrDefault(e => !string.IsNullOrEmpty(e.ProjectName))?.ProjectName ?? "";

            IEnumerable<ErrorListItem> cleanErrors = errors.Where(e => e != null && !string.IsNullOrEmpty(e.FileName));

            lock (_snapshots)
            {
                foreach (IGrouping<string?, ErrorListItem>? fileErrorMap in cleanErrors.GroupBy(e => e.FileName))
                {
                    if (fileErrorMap.Key != null)
                    {
                        if (_snapshots.ContainsKey(fileErrorMap.Key))
                        {
                            IEnumerable<ErrorListItem> values = cleanErrors.Where(e => e.FileName == fileErrorMap.Key);
                            _snapshots[fileErrorMap.Key].Update(values);
                        }
                        else
                        {
                            _snapshots[fileErrorMap.Key] = new TableEntriesSnapshot(projectName, fileErrorMap.Key, fileErrorMap);
                        }
                    }
                }
            }

            UpdateAllSinks();
        }

        /// <summary>
        /// Clears all previously registered issues/errors
        /// </summary>
        public void CleanAllErrors()
        {
            lock (_snapshots)
            {
                lock (_managers)
                {
                    foreach (SinkManager manager in _managers)
                    {
                        manager.Clear();
                    }
                }

                foreach (TableEntriesSnapshot snapshot in _snapshots.Values)
                {
                    snapshot.Dispose();
                }

                _snapshots.Clear();
            }
        }
    }
}
