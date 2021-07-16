using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;

namespace Community.VisualStudio.Toolkit.DependencyInjection
{
    /// <summary>
    /// Package that contains a DI service container.
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    [ProvideService(typeof(SToolkitServiceProvider<>), IsAsyncQueryable = true)]
    public abstract class DIToolkitPackage<TPackage> : ToolkitPackage
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

            IServiceCollection services = this.CreateServiceCollection();
            InitializeServices(services);

            IServiceProvider serviceProvider = BuildServiceProvider(services);
            ServiceProvider = new ToolkitServiceProvider<TPackage>(serviceProvider);

            // Add the IToolkitServiceProvider to the VS IServiceProvider
            AsyncServiceCreatorCallback serviceCreatorCallback = (sc, ct, t) =>
            {
                return Task.FromResult((object)this.ServiceProvider);
            };

            AddService(typeof(SToolkitServiceProvider<TPackage>), serviceCreatorCallback, true);
        }

        /// <summary>
        /// Create the service collection.
        /// </summary>
        /// <returns></returns>
        protected abstract IServiceCollection CreateServiceCollection();

        /// <summary>
        /// Builds the service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        protected abstract IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection);

        /// <summary>
        /// Initialize the services in the DI container.
        /// </summary>
        /// <param name="services"></param>
        protected virtual void InitializeServices(IServiceCollection services)
        {
            // Nothing
        }
    }
}
