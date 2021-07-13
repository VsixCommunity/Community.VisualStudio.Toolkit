using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit.Shared.DependencyInjection
{
    internal class ToolkitServiceProvider<TPackage> : IToolkitServiceProvider<TPackage>
         where TPackage : AsyncPackage
    {
        private readonly IServiceProvider _serviceProvider;

        public ToolkitServiceProvider(ServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
