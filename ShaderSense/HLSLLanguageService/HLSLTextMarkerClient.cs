using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

namespace Babel
{
    abstract class HLSLTextMarkerClient : IVsTextMarkerClient
    {

        #region IVsTextMarkerClient Members

        public int ExecMarkerCommand(IVsTextMarker pMarker, int iItem)
        {
            throw new NotImplementedException();
        }

        public int GetMarkerCommandInfo(IVsTextMarker pMarker, int iItem, string[] pbstrText, uint[] pcmdf)
        {
            throw new NotImplementedException();
        }

        public void MarkerInvalidated()
        {
            throw new NotImplementedException();
        }

        public int OnAfterMarkerChange(IVsTextMarker pMarker)
        {
            throw new NotImplementedException();
        }

        public void OnAfterSpanReload()
        {
            throw new NotImplementedException();
        }

        public void OnBeforeBufferClose()
        {
            throw new NotImplementedException();
        }

        public void OnBufferSave(string pszFileName)
        {
            throw new NotImplementedException();
        }

        abstract public int GetTipText(IVsTextMarker pMarker, string[] pbstrText);

        #endregion
    }

    class HLSLIdentifierTextMarkerClient : HLSLTextMarkerClient
    {
        public override int GetTipText(IVsTextMarker pMarker, string[] pbstrText)
        {
            pbstrText[0] = "Have you declared this identifier in this scope?";
            return VSConstants.S_OK;
        }
    }

    class HLSLFunctionTextMarkerClient : HLSLTextMarkerClient
    {
        public override int GetTipText(IVsTextMarker pMarker, string[] pbstrText)
        {
            pbstrText[0] = "Has this function been declared?";
            return VSConstants.S_OK;
        }
    }
}
