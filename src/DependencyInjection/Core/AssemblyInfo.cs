// This will add the toolkit assembly to Visual Studio's probing path.
// Without it, Visual Studio is unable to find the assembly and the extension will fail to load.
using Microsoft.VisualStudio.Shell;

[assembly: ProvideCodeBase(AssemblyName = "Community.VisualStudio.Toolkit")]
[assembly: ProvideCodeBase(AssemblyName = "Community.VisualStudio.Toolkit.DependencyInjection")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.Extensions.DependencyInjection")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.Extensions.DependencyInjection.Abstractions")]