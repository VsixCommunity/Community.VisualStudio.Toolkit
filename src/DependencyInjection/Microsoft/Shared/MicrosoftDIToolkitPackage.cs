using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit.DependencyInjection.Microsoft
{
    /// <summary>
    /// Package with a Microsoft dependency injection implementation.
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    public class MicrosoftDIToolkitPackage<TPackage> : DIToolkitPackage<TPackage>
        where TPackage : AsyncPackage
    {
        /// <inheritdoc/>
        protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
        {
            if (!(serviceCollection is ServiceCollection services))
                throw new Exception($"The '{nameof(IServiceCollection)}' must be of type '{typeof(ServiceCollection).FullName}'.");

            return services.BuildServiceProvider();
        }

        /// <inheritdoc/>
        protected override IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }
    }
}
