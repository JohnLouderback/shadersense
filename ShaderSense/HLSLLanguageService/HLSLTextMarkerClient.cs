/**************************************************
 * 
 * Copyright 2009 Garrett Kiel
 * 
 * This file is part of Shader Sense.
 *
 *   Shader Sense is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Shader Sense is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Shader Sense.  If not, see <http://www.gnu.org/licenses/>.
 *
 *************************************************/

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
