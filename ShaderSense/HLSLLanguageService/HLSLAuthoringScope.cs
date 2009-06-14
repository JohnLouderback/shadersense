/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Babel
{
    /* HLSLAuthoringScope
     * Stores references to the source and lists of methods. Represents the overall scope of the
     * document.
     */
    public class HLSLAuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
    {
        HLSLSource _source;
        HLSLFunctions methods;
        object parseResult;
        IASTResolver resolver;

        //constructor
        public HLSLAuthoringScope(HLSLSource source)
        {
            this._source = source;
            this.resolver = new HLSLResolver();
        }

        //constructor
        public HLSLAuthoringScope(object parseResult)
        {
            this.parseResult = parseResult;

            // how should this be set?
            this.resolver = new HLSLResolver();
        }


        /* ParseReason.QuickInfo
         * Visual Studio calls this function which get tool tip info by
         * calling the get decription function
         */
        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            TokenInfo tokenInfo = this._source.GetTokenInfo(line, col);
            
            span = new TextSpan();
            span.iStartLine = line;
            span.iEndLine = line;
            span.iStartIndex = tokenInfo.StartIndex;
            span.iEndIndex = tokenInfo.EndIndex + 1;

            string tokenFound = this._source.GetText(span);


            return Babel.Lexer.Scanner.GetDescriptionForTokenValue(tokenFound);
        }

        // ParseReason.CompleteWord
        // ParseReason.DisplayMemberList
        // ParseReason.MemberSelect
        // ParseReason.MemberSelectAndHilightBraces
        /*
         * Visual Studio calls this function to get possible declarations and completions
         */
        public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            string currentCommand;
            string startChar;
            //int hResult = view.GetTextStream(line, info.StartIndex, line, info.EndIndex, out currentCommand);
            //int hResult = view.GetTextStream(line, info.StartIndex, line, col, out currentCommand);
            TextSpan[] ts = new TextSpan[1];
            ts[0] = new TextSpan();
            view.GetWordExtent(line, info.StartIndex, (uint)(WORDEXTFLAGS.WORDEXT_FINDTOKEN), ts);
            int hResult = view.GetTextStream(line, ts[0].iStartIndex, line, ts[0].iEndIndex, out currentCommand);
            if (ts[0].iStartIndex > 0)
            {
                view.GetTextStream(line, ts[0].iStartIndex - 1, line, ts[0].iStartIndex, out startChar);
                if (startChar.Equals("#"))
                    currentCommand = startChar;
            }


            ((HLSLResolver)resolver)._source = (HLSLSource)_source;
            IList<HLSLDeclaration> declarations;
            switch (reason)
            {
                case ParseReason.CompleteWord:
                    declarations = resolver.FindCompletions(currentCommand, line, col);
                    break;
                case ParseReason.DisplayMemberList:
                case ParseReason.MemberSelect:
                case ParseReason.MemberSelectAndHighlightBraces:
                    if(currentCommand.Equals("."))
                        declarations = resolver.FindMembers(parseResult, line, col);
                    else
                        declarations = resolver.FindCompletions(currentCommand, line, col);
                    break;
                default:
                    throw new ArgumentException("reason");
            }


            return new HLSLDeclarations(declarations);
        }

        // ParseReason.GetMethods
        // called by visual studio to get methods
        public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name)
        {
            return new HLSLFunctions(resolver.FindMethods(parseResult, line, col, name));
        }

        // ParseReason.Goto
        // Called by Visual Studio to get the go to function
        // this has not been implemented yet
        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            // throw new System.NotImplementedException();
            span = new TextSpan();
            return null;
        }

        //update methods
        public void updateMethods()
        {
            //methods = new HLSLFunctions(new List<HLSLFunction>(Parser.Parser.methods));
            methods = new HLSLFunctions(new List<HLSLFunction>(_source.methods));
        }
    }
}