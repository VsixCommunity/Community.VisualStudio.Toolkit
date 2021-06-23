using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of events.</summary>
    public class Events
    {
        private readonly Events2? _events;

        internal Events()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            _events = (Events2)dte.Events;    
        }

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public BuildEvents BuildEvents => new();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private CodeModelEvents? _codeModelEvents;
        public CodeModelEvents? CodeModelEvents => _codeModelEvents ??= _events?.CodeModelEvents;


        private CommandEvents? _commandEvents;
        public CommandEvents? CommandEvents => _commandEvents ??= _events?.CommandEvents;


        /// <summary>
        /// Events related to the selection in Visusal Studio.
        /// </summary>
        public DebuggerEvents DebuggerEvents => new();


        private DebuggerExpressionEvaluationEvents? _debuggerExpressionEvaluationEvents;
        public DebuggerExpressionEvaluationEvents? DebuggerExpressionEvaluationEvents => _debuggerExpressionEvaluationEvents ??= _events?.DebuggerExpressionEvaluationEvents;


        private DebuggerProcessEvents? _debuggerProcessEvents;
        public DebuggerProcessEvents? DebuggerProcessEvents => _debuggerProcessEvents ??= _events?.DebuggerProcessEvents;

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public DocumentEvents DocumentEvents => new();


        private DTEEvents? _dteEvents;
        public DTEEvents? DTEEvents => _dteEvents ??= _events?.DTEEvents;


        private FindEvents? _findEvents;
        public FindEvents? FindEvents => _findEvents ??= _events?.FindEvents;


        private ProjectItemsEvents? _miscFilesEvents;
        public ProjectItemsEvents? MiscFilesEvents => _miscFilesEvents ??= _events?.MiscFilesEvents;


        private OutputWindowEvents? _outputWindowEvents;
        public OutputWindowEvents? OutputWindowEvents => _outputWindowEvents ??= _events?.OutputWindowEvents;


        private ProjectItemsEvents? _projectItemEvents;
        public ProjectItemsEvents? ProjectItemEvents => _projectItemEvents ??= _events?.ProjectItemsEvents;


        private ProjectsEvents? _projectEvents;
        public ProjectsEvents? ProjectEvents => _projectEvents ??= _events?.ProjectsEvents;


        private PublishEvents? _publishEvents;
        public PublishEvents? PublishEvents => _publishEvents ??= _events?.PublishEvents;

        /// <summary>
        /// Events related to the selection in Visusal Studio
        /// </summary>
        public SelectionEvents? SelectionEvents => new();

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public SolutionEvents SolutionEvents => new();


        private ProjectItemsEvents? _solutionItemEvents;
        public ProjectItemsEvents? SolutionItemEvents => _solutionItemEvents ??= _events?.SolutionItemsEvents;


        private TaskListEvents? _taskListEvents;
        public TaskListEvents? TaskListEvents => _taskListEvents ??= _events?.TaskListEvents;


        private TextDocumentKeyPressEvents? _textDocumentKeyPressEvents;
        public TextDocumentKeyPressEvents? TextDocumentKeyPressEvents => _textDocumentKeyPressEvents ??= _events?.TextDocumentKeyPressEvents;


        private TextEditorEvents? _textEditorEvents;
        public TextEditorEvents? TextEditorEvents => _textEditorEvents ??= _events?.TextEditorEvents;


        private WindowEvents? _windowEvents;
        public WindowEvents? WindowEvents => _windowEvents ??= _events?.WindowEvents;


        private WindowVisibilityEvents? _windowVisibilityEvents;
        public WindowVisibilityEvents? WindowVisibilityEvents => _windowVisibilityEvents ??= _events?.WindowVisibilityEvents;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
