using System;
using System.IO;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    public sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            FullPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(FullPath);
        }

        public string FullPath { get; }

        public void Dispose()
        {
            Directory.Delete(FullPath, true);
        }
    }
}
