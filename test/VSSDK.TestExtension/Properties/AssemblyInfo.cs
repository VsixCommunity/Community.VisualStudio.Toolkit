using Microsoft.VisualStudio.Shell;

[assembly: ProvideCodeBase(AssemblyName = "Community.VisualStudio.Toolkit")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.Extensions.DependencyInjection", CodeBase = "$PackageFolder$\\Microsoft.Extensions.DependencyInjection.dll")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.Extensions.DependencyInjection.Abstractions", CodeBase = "$PackageFolder$\\Microsoft.Extensions.DependencyInjection.Abstractions.dll")]
