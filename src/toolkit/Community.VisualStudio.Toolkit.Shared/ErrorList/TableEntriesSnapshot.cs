using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Community.VisualStudio.Toolkit
{
    internal class TableEntriesSnapshot : TableEntriesSnapshotBase
    {
        private readonly int _versionNumber = 1;
        private readonly IList<ErrorListItem> _errors;

        public TableEntriesSnapshot(string projectName, string filePath, IEnumerable<ErrorListItem> errors)
        {
            ProjectName = projectName;
            FilePath = filePath;
            _errors = errors.ToList();
        }

        public string ProjectName { get; private set; }

        public string FilePath { get; private set; }

        public override int Count
        {
            get { return _errors.Count; }
        }

        public override int VersionNumber
        {
            get { return _versionNumber; }
        }

        public override bool TryGetValue(int index, string columnName, out object? content)
        {
            if ((index < 0) || (index >= Count))
            {
                content = null;
                return false;
            }

            switch (columnName)
            {
                case StandardTableKeyNames.ProjectName:
                    content = ProjectName;
                    break;

                case StandardTableKeyNames.DocumentName:
                    content = FilePath;
                    break;

                case StandardTableKeyNames.Text:
                    content = _errors[index].Message;
                    break;

                case StandardTableKeyNames.Line:
                    content = _errors[index].Line;
                    break;

                case StandardTableKeyNames.Column:
                    content = _errors[index].Column;
                    break;

                case StandardTableKeyNames.ErrorCategory:
                    content = _errors[index].ErrorCategory;
                    break;

                case StandardTableKeyNames.ErrorSeverity:
                    content = _errors[index].Severity;
                    break;

                case StandardTableKeyNames.ErrorCode:
                    content = _errors[index].ErrorCode;
                    break;

                case StandardTableKeyNames.BuildTool:
                    content = _errors[index].BuildTool;
                    break;

                case StandardTableKeyNames.HelpLink:
                    content = _errors[index].HelpLink;
                    break;

                case StandardTableKeyNames.ErrorCodeToolTip:
                    content = _errors[index].ErrorCodeToolTip;
                    break;

                default:
                    content = null;
                    return false;
            }

            return true;
        }
    }
}
