using System.Threading;
using System;
using Community.VisualStudio.Toolkit.Shared.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Package that contains a DI service container.
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    [ProvideService(typeof(SToolkitServiceProvider<>), IsAsyncQueryable = true)]
    public class DependencyInjectionContainerToolkitPackage<TPackage> : ToolkitPackage
        where TPackage : AsyncPackage
    {
        /// <summary>
        /// Custom ServiceProvider for the package.
        /// </summary>
        public IToolkitServiceProvider<TPackage> ServiceProvider { get; private set; } = null!; // This property is initialized in `InitializeAsync`, so it's never actually null.

        /// <summary>
        /// Initializes the <see cref="AsyncPackage"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            ServiceCollection services = new ServiceCollection();
            InitializeServices(services);
            ServiceProvider = new ToolkitServiceProvider<TPackage>(services);

            // Add the IToolkitServiceProvider to the VS IServiceProvider
            AsyncServiceCreatorCallback serviceCreatorCallback = (sc, ct, t) =>
            {
                return Task.FromResult((object)this.ServiceProvider);
            };

            AddService(typeof(SToolkitServiceProvider<TPackage>), serviceCreatorCallback, true);
        }

        /// <summary>
        /// Initialize the services in the DI container.
        /// </summary>
        /// <param name="services"></param>
        protected virtual void InitializeServices(ServiceCollection services)
        {
            // Nothing
        }
    }
}
