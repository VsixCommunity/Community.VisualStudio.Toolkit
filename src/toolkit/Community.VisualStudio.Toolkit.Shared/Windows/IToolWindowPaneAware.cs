using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Allows the content of a <see cref="ToolWindowPane"/> to be aware of its owning <see cref="ToolWindowPane"/> object.
    /// <para>
    /// Implement this interface on your tool window's content (the object you return from 
    /// <see cref="BaseToolWindow{T}.CreateAsync(int, System.Threading.CancellationToken)"/>)
    /// if you need the content object to have access to its <see cref="ToolWindowPane"/>.
    /// </para>
    /// </summary>
    public interface IToolWindowPaneAware
    {
        /// <summary>
        /// Called when the <see cref="ToolWindowPane"/> has been initialized and "sited". 
        /// The pane's service provider can be used from this point onwards.
        /// </summary>
        /// <param name="pane">The tool window pane that was created.</param>
        void SetPane(ToolWindowPane pane);
    }
}
