using System.Collections.Generic;
using System.Linq;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    public class CompileOptions
    {
        public string Target { get; set; } = "Build";
        public object Properties { get; set; } = new object();
        public IEnumerable<string> Arguments { get; set; } = Enumerable.Empty<string>();
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
    }
}
