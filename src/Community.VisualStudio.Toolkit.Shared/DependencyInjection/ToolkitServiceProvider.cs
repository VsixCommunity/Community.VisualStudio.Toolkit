using System;
using Microsoft.Extensions.DependencyInjection;

namespace Community.VisualStudio.Toolkit.Shared.DependencyInjection
{
    internal class ToolkitServiceProvider : IToolkitServiceProvider
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
