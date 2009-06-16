using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

namespace Babel
{
    public class HLSLViewFilter : ViewFilter
    {
        public HLSLViewFilter(CodeWindowManager mgr, IVsTextView view)
            : base(mgr, view)
        {
        }

        public override bool HandlePreExec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (guidCmdGroup == VSConstants.VSStd2K)
            {
                if ((VSConstants.VSStd2KCmdID)nCmdId == VSConstants.VSStd2KCmdID.ECMD_LEFTCLICK)
                    base.Source.OnCommand(base.TextView, (VSConstants.VSStd2KCmdID)nCmdId, '\0');
            }

            return base.HandlePreExec(ref guidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        }

        public override void HandlePostExec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, bool bufferWasChanged)
        {
            if (guidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdId)
                {
                    case VSConstants.VSStd2KCmdID.LEFT_EXT_COL:
                    case VSConstants.VSStd2KCmdID.RIGHT_EXT_COL:
                    case VSConstants.VSStd2KCmdID.UP:
                    case VSConstants.VSStd2KCmdID.UP_EXT:
                    case VSConstants.VSStd2KCmdID.UP_EXT_COL:
                    case VSConstants.VSStd2KCmdID.DOWN:
                    case VSConstants.VSStd2KCmdID.DOWN_EXT:
                    case VSConstants.VSStd2KCmdID.DOWN_EXT_COL:
                    case VSConstants.VSStd2KCmdID.HOME:
                    case VSConstants.VSStd2KCmdID.HOME_EXT:
                    case VSConstants.VSStd2KCmdID.BOL:
                    case VSConstants.VSStd2KCmdID.BOL_EXT:
                    case VSConstants.VSStd2KCmdID.BOL_EXT_COL:
                    case VSConstants.VSStd2KCmdID.FIRSTCHAR:
                    case VSConstants.VSStd2KCmdID.FIRSTCHAR_EXT:
                    case VSConstants.VSStd2KCmdID.EOL:
                    case VSConstants.VSStd2KCmdID.EOL_EXT:
                    case VSConstants.VSStd2KCmdID.EOL_EXT_COL:
                    case VSConstants.VSStd2KCmdID.LASTCHAR:
                    case VSConstants.VSStd2KCmdID.LASTCHAR_EXT:
                    case VSConstants.VSStd2KCmdID.PAGEUP:
                    case VSConstants.VSStd2KCmdID.PAGEUP_EXT:
                    case VSConstants.VSStd2KCmdID.PAGEDN:
                    case VSConstants.VSStd2KCmdID.PAGEDN_EXT:
                    case VSConstants.VSStd2KCmdID.WORDNEXT:
                    case VSConstants.VSStd2KCmdID.WORDNEXT_EXT:
                    case VSConstants.VSStd2KCmdID.WORDNEXT_EXT_COL:
                    case VSConstants.VSStd2KCmdID.WORDPREV:
                    case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
                    case VSConstants.VSStd2KCmdID.WORDPREV_EXT_COL:
                    case VSConstants.VSStd2KCmdID.BOTTOMLINE:
                    case VSConstants.VSStd2KCmdID.BOTTOMLINE_EXT:
                    case VSConstants.VSStd2KCmdID.TOPLINE:
                    case VSConstants.VSStd2KCmdID.TOPLINE_EXT:
                        // check general trigger characters for intellisense
                        base.Source.OnCommand(base.TextView, (VSConstants.VSStd2KCmdID)nCmdId, '\0');
                        break;
                }
            }

            base.HandlePostExec(ref guidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut, bufferWasChanged);
        }
    }
}
