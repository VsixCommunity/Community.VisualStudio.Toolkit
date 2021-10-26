using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    internal class DynamicItemMenuCommand : OleMenuCommand
    {
        private readonly Func<int, bool> _isMatch;

        public DynamicItemMenuCommand(Func<int, bool> isMatch, EventHandler invoke, EventHandler beforeQueryStatus, CommandID id) : base(invoke, changeHandler: null, beforeQueryStatus, id)
        {
            _isMatch = isMatch;
        }

        public override bool DynamicItemMatch(int cmdId)
        {
            if (_isMatch(cmdId))
            {
                MatchedCommandId = cmdId;
                return true;
            }

            MatchedCommandId = 0;
            return false;
        }
    }
}