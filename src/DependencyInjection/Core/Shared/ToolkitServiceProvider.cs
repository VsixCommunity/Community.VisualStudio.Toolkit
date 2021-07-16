using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit.DependencyInjection.Core
{
    internal class ToolkitServiceProvider<TPackage> : IToolkitServiceProvider<TPackage>
         where TPackage : AsyncPackage
    {
        private readonly IServiceProvider _serviceProvider;

        public ToolkitServiceProvider(IServiceProvider services)
        {
            _serviceProvider = services;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
