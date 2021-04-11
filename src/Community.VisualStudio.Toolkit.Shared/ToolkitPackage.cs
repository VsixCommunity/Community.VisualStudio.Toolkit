using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An <see cref="AsyncPackage"/> that provides additional functionality.
    /// </summary>
    public abstract class ToolkitPackage : AsyncPackage
    {
        private List<IToolWindowProvider>? _toolWindowProviders;

        internal void AddToolWindow(IToolWindowProvider provider)
        {
            if (_toolWindowProviders is null)
            {
                _toolWindowProviders = new();
            }

            _toolWindowProviders.Add(provider);
        }

        private IToolWindowProvider? GetToolWindowProvider(Type toolWindowType)
        {
            return _toolWindowProviders?.FirstOrDefault(x => toolWindowType == x.PaneType);
        }

#if VS16
        /// <inheritdoc/>
        public override IVsAsyncToolWindowFactory? GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            if (_toolWindowProviders is not null)
            {
                return _toolWindowProviders.Any(x => toolWindowType == x.PaneType.GUID) ? this : null;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            return GetToolWindowProvider(toolWindowType)?.GetTitle(id) ?? base.GetToolWindowTitle(toolWindowType, id);
        }

        /// <inheritdoc/>
        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            IToolWindowProvider? provider = GetToolWindowProvider(toolWindowType);

            if (provider is not null)
            {
                return new InstantiateToolWindowData(id, await provider.CreateAsync(id, cancellationToken));
            }
            else
            {
                return await base.InitializeToolWindowAsync(toolWindowType, id, cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override WindowPane InstantiateToolWindow(Type toolWindowType, object context)
        {
            IToolWindowProvider? provider = GetToolWindowProvider(toolWindowType);

            if (provider is not null)
            {
                // The context object we have been given was returned from the `InitializeToolWindowAsync()`
                // method, and is an `InstantiateToolWindowData` object that contains the content for the tool 
                // window and its ID. The tool window is always created with an unspecified context object.
                var data = (InstantiateToolWindowData)context;
                WindowPane pane = base.InstantiateToolWindow(toolWindowType, ToolWindowCreationContext.Unspecified);

                // Set the content and the caption of the tool window. This needs to be 
                // done as part of initializing the tool window. Doing it later results in
                // errors being thrown when Visual Studio creates the tool window's frame.
                pane.Content = data.Content;

                if (pane is ToolWindowPane toolPane)
                {
                    toolPane.Caption = provider.GetTitle(data.Id);
                }

                return pane;
            }
            else
            {
                return base.InstantiateToolWindow(toolWindowType, context);
            }
        }

        private class InstantiateToolWindowData
        {
            public InstantiateToolWindowData(int id, FrameworkElement content)
            {
                Id = id;
                Content = content;
            }

            public int Id { get; }

            public FrameworkElement Content { get; }
        }
#else
        private int _currentToolWindowId = 0;

        /// <inheritdoc/>
        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            // Remember the tool ID so that we can use it in `InstantiateToolWindow()`.
            _currentToolWindowId = id;

            try
            {
                return base.CreateToolWindow(ref toolWindowType, id);
            }
            finally
            {
                _currentToolWindowId = 0;
            }
        }

        /// <inheritdoc/>
        protected override WindowPane CreateToolWindow(Type toolWindowType, int id)
        {
            // Remember the tool ID so that we can use it in `InstantiateToolWindow()`.
            _currentToolWindowId = id;

            try
            {
                return base.CreateToolWindow(toolWindowType, id);
            }
            finally
            {
                _currentToolWindowId = 0;
            }
        }

        /// <inheritdoc/>
        protected override WindowPane? InstantiateToolWindow(Type toolWindowType)
        {
            WindowPane windowPane = base.InstantiateToolWindow(toolWindowType);
            IToolWindowProvider? provider = GetToolWindowProvider(toolWindowType);

            if (provider is not null)
            {
                // Set the content and the caption of the tool window. This needs to be 
                // done as part of initializing the tool window. Doing it later results in
                // errors being thrown when Visual Studio creates the tool window's frame.
                windowPane.Content = ThreadHelper.JoinableTaskFactory.Run(
                    () => provider.CreateAsync(_currentToolWindowId, DisposalToken)
                );

                if (windowPane is ToolWindowPane toolPane)
                {
                    toolPane.Caption = provider.GetTitle(_currentToolWindowId);
                }
            }

            return windowPane;
        }
#endif
    }
}
