/**************************************************
 * 
 * Copyright 2009 Garrett Kiel, Cory Luitjohan, Feng Cao, Phil Slama, Ed Han, Michael Covert
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
using Microsoft.VisualStudio.Package;
using System.Runtime.InteropServices;
using Company.ShaderSense;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{
    /*HLSLLanguageService
     * Runs the parse; has a few extra steps to deal with the parse reason.
     * Calls the class it inherits from (provided code) for most cases.
     */

    [ComVisible(true)]
    [Guid(GuidList.guidHLSLLanguageService)]
    public class HLSLLanguageService : BabelLanguageService
    {
        //get format filter
        public override string GetFormatFilterList()
        {
            return Babel.Configuration.FormatList;
        }

        //parses the source
        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest req)
        {
           Source source = (Source)this.GetSource(req.FileName);

            if( req.Reason == ParseReason.Check )
                req.Callback = OnParseComplete;
            switch (req.Reason)
            {
                case ParseReason.QuickInfo:
                    return new HLSLAuthoringScope(source);
                case ParseReason.CompleteWord:
                    return new HLSLAuthoringScope(source);
                case ParseReason.MethodTip:
                    HLSLAuthoringScope scope = (HLSLAuthoringScope)base.ParseSource(req);
                    scope.updateMethods();
                    if (req.TokenInfo.Trigger == TokenTriggers.ParameterStart)
                    {
                        //  Trigger the method start if the trigger was a parameter start
                        KeyValuePair<TextSpan, string> method = new KeyValuePair<TextSpan, string>(new TextSpan(), "");
                        foreach (KeyValuePair<TextSpan, string> funckv in Parser.Parser.funcNamesLocs)
                        {
                            if (TextSpanHelper.IsAfterEndOf(funckv.Key, req.Line, req.Col) && TextSpanHelper.StartsAfterEndOf(funckv.Key, method.Key))
                                method = funckv;
                        }
                        bool isFunction = false;
                        foreach (HLSLFunction func in Parser.Parser.methods)
                        {
                            if (method.Value.Equals(func.Name))
                            {
                                isFunction = true;
                                break;
                            }
                        }
                        if( isFunction )
                            startMethodTip(source, req.Sink, req.Line, req.Col);
                    }
                    else
                    {
                        //  Logic to find out what the current state of the parse is
                        string line = source.GetText(req.Line, 0, req.Line, req.Col - 1);
                        int paramStart = line.LastIndexOf('(');
                        string rawParamList = line.Substring(paramStart + 1);
                        rawParamList.TrimEnd(')');
                        string[] paramList = rawParamList.Split(',');
                        //  Find start of method call
                        if (!startMethodTip(source, req.Sink, req.Line, paramStart + 1)) return scope;
                        //  Fine start of parameter list
                        for (int i = paramStart, k = 0; k < paramList.Length - 1; k++)
                        {
                            i += paramList[k].Length;
                            TextSpan span = new TextSpan();
                            span.iStartLine = span.iEndLine = req.Line;
                            span.iStartIndex = ++i;
                            span.iEndIndex = i + 1;
                            string text = source.GetText(span);
                            req.Sink.NextParameter(span);
                        }
                        //  Find closing parenthesis
                        if (req.TokenInfo.Trigger == TokenTriggers.ParameterEnd)
                        {
                            TextSpan span = new TextSpan();
                            span.iStartLine = span.iEndLine = req.Line;
                            span.iStartIndex = req.Col - 1;
                            span.iEndIndex = req.Col;
                            string text = source.GetText(span);
                            req.Sink.EndParameters(span);
                        }
                    }
                    return scope;
                default:
                    return base.ParseSource(req);
            }
        }

        //  TODO: consider multi-line function calls
        //  Parses starting from before line and col looking for the function name to look up
        private bool startMethodTip(Source source, AuthoringSink sink, int line, int col)
        {
            TextSpan span = new TextSpan();
            span.iStartLine = span.iEndLine = line;
            if (col <= 0)
            {
                return false;
            }
            span.iStartIndex = col - 1;
            span.iEndIndex = col;
            string pStart = source.GetText(span);
            if (pStart != "(")
            {
                return false;
            }
            TokenInfo funToken = source.GetTokenInfo(line, col - 1);
            TextSpan funSpan = new TextSpan();
            funSpan.iStartLine = funSpan.iEndLine = span.iStartLine;
            funSpan.iStartIndex = funToken.StartIndex;
            funSpan.iEndIndex = span.iStartIndex;
            string funName = source.GetText(funSpan);
            sink.StartName(funSpan, funName);
            sink.StartParameters(span);
            return true;
        }

        private IScanner scanner;
        //creates the scanner for scanning the source in the parser
        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (scanner == null)
                this.scanner = new HLSLLineScanner();

            return this.scanner;
        }

        private static HLSLIdentifierTextMarkerClient pIdentClient = new HLSLIdentifierTextMarkerClient();
        private static HLSLFunctionTextMarkerClient pFuncClient = new HLSLFunctionTextMarkerClient();
        public override void OnParseComplete(ParseRequest req)
        {
            base.OnParseComplete(req);

            IVsTextLines textlines;
            req.View.GetBuffer(out textlines);
            //Clean up the old textmarkers in case some of them are no longer needed
            IVsEnumLineMarkers ppEnum;
            textlines.EnumMarkers(0, 0, 0, 0, 0, (uint)(ENUMMARKERFLAGS.EM_ALLTYPES | ENUMMARKERFLAGS.EM_ENTIREBUFFER), out ppEnum);
            IVsTextLineMarker ppRetval;
            ppEnum.Next(out ppRetval);
            while (ppRetval != null)
            {
                ppRetval.Invalidate();
                ppEnum.Next(out ppRetval);
            }

            foreach( KeyValuePair<TextSpan, string> identkv in Parser.Parser.identNamesLocs)
            {
                int line, col;
                line = identkv.Key.iStartLine;
                col = identkv.Key.iStartIndex;
                Dictionary<string, Parser.Parser.VarDecl> vars = new Dictionary<string, Babel.Parser.Parser.VarDecl>();
                Parser.Parser.CodeScope curCS = HLSLScopeUtils.GetCurrentScope(Parser.Parser.programScope, line, col);
                if (curCS == null)
                    curCS = Parser.Parser.programScope;
                HLSLScopeUtils.GetVarDecls(curCS, vars);
                bool isValid = false;
                foreach (KeyValuePair<string, Parser.Parser.VarDecl> kv in vars)
                {
                    if(kv.Key.Equals(identkv.Value))
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    TextSpan ts = identkv.Key;
                    textlines.CreateLineMarker((int)MARKERTYPE.MARKER_CODESENSE_ERROR, ts.iStartLine, ts.iStartIndex, ts.iEndLine, ts.iEndIndex, pIdentClient, null);
                }
            }

            foreach (KeyValuePair<TextSpan, string> funckv in Parser.Parser.funcNamesLocs)
            {
                int line, col;
                line = funckv.Key.iStartLine;
                col = funckv.Key.iStartIndex;
                bool isValid = false;
                foreach (HLSLFunction func in Parser.Parser.methods)
                {
                    if (func.Name.Equals(funckv.Value))
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    TextSpan ts = funckv.Key;
                    textlines.CreateLineMarker((int)MARKERTYPE.MARKER_CODESENSE_ERROR, ts.iStartLine, ts.iStartIndex, ts.iEndLine, ts.iEndIndex, pFuncClient, null);
                }
            }
        }
    }
}
