using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Community.VisualStudio.Toolkit
{
    internal interface IToolWindowProvider
    {
        public string GetTitle(int toolWindowId);

        public Type PaneType { get; }

        public Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken);
    }
}
