using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit.DependencyInjection.Core
{
    /// <summary>
    /// Placeholder interface for registering the <see cref="IToolkitServiceProvider{TPackage}"/> in the main Visual Studio service provider.
    /// </summary>
    /// <typeparam name="TPackage">Type of the implementing package.</typeparam>
    public interface SToolkitServiceProvider<TPackage>
        where TPackage : AsyncPackage
    {
    }
}
