namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of events.</summary>
    public class Events
    {
        internal Events()
        { }

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public BuildEvents BuildEvents => new();

        /// <summary>
        /// Events related to the selection in Visusal Studio.
        /// </summary>
        public DebuggerEvents DebuggerEvents => new();

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public DocumentEvents DocumentEvents => new();

        /// <summary>
        /// Events related to the Visual Studio Shell.
        /// </summary>
        public ShellEvents ShellEvents => new();

        /// <summary>
        /// Events related to the selection in Visusal Studio
        /// </summary>
        public SelectionEvents? SelectionEvents => new();

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public SolutionEvents SolutionEvents => new();
    }
}
