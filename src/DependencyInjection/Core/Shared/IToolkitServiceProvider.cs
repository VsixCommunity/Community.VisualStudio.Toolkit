﻿using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit.DependencyInjection.Core
{
    /// <summary>
    /// Service provider
    /// </summary>
    /// <typeparam name="TPackage">Type of the implementing package.</typeparam>
    public interface IToolkitServiceProvider<TPackage> : IServiceProvider
         where TPackage : AsyncPackage
    {
    }
}
