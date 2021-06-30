namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of events.</summary>
    public partial class Events
    {
        internal Events()
        { }

        /// <summary>
        /// Events related to the selection in Visusal Studio
        /// </summary>
        public SelectionEvents? SelectionEvents => new();
    }
}
