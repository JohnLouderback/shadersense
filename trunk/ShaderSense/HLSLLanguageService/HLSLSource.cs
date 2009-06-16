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
using Microsoft.VisualStudio.Package;
using Babel.Parser;

namespace Babel
{
    public class HLSLSource : Source
    {
        public CodeScope programScope;

        public List<HLSLFunction> methods;
        //TODO: Don't forget to change the reference in lexer.lex
        public Dictionary<string, StructMembers> structDecls;
        public List<HLSLDeclaration> typedefTypes;
        public Dictionary<TextSpan, string> identNamesLocs;
        public Dictionary<TextSpan, string> funcNamesLocs;
        public Dictionary<TextSpan, LexValue> structVars;
        public List<string> includeFiles;
        private Dictionary<string, HLSLSource> allIncludes;

        public HLSLSource(HLSLLanguageService service, IVsTextLines textLines, Colorizer colorizer)
			: base(service, textLines, colorizer)
		{
		}

        public void PrepareParse(TextSpan programLoc)
        {
            programScope = new CodeScope(programLoc);
            methods = new List<HLSLFunction>();
            structDecls = new Dictionary<string, StructMembers>();
            typedefTypes = new List<HLSLDeclaration>();
            identNamesLocs = new Dictionary<TextSpan, string>();
            funcNamesLocs = new Dictionary<TextSpan, string>();
            structVars = new Dictionary<TextSpan, LexValue>();
            includeFiles = new List<string>();
        }

        public void GatherIncludes()
        {
            allIncludes = new Dictionary<string, HLSLSource>();
            GatherIncludes(allIncludes);
        }

        private void GatherIncludes(Dictionary<string, HLSLSource> includes)
        {
            foreach (string s in includeFiles)
            {
                if (!includes.ContainsKey(s))
                {
                    HLSLSource source = (HLSLSource)LanguageService.GetSource(s);
                    includes.Add(s, source);
                    source.GatherIncludes(includes);
                }
            }
        }

        public void GatherStructDecls(Dictionary<string, StructMembers> decls)
        {
            foreach (KeyValuePair<string, StructMembers> kv in structDecls)
                decls.Add(kv.Key, kv.Value);

            foreach (KeyValuePair<string, HLSLSource> kv in allIncludes)
            {
                foreach (KeyValuePair<string, StructMembers> sm in kv.Value.structDecls)
                {
                    if (!decls.ContainsKey(sm.Key))
                    {
                        decls.Add(sm.Key, sm.Value);
                    }
                }
            }
        }

        public void GatherVariables(CodeScope cs, Dictionary<string, Babel.Parser.VarDecl> vars)
        {
            HLSLScopeUtils.GetVarDecls(cs, vars);

            foreach (KeyValuePair<string, HLSLSource> kv in allIncludes)
            {
                foreach (KeyValuePair<string, VarDecl> decls in kv.Value.programScope.scopeVars)
                {
                    if (!vars.ContainsKey(decls.Key))
                    {
                        vars.Add(decls.Key, decls.Value);
                    }
                }
            }
        }

        public void GatherFunctions(List<HLSLFunction> funcs)
        {
            foreach (HLSLFunction f in methods)
                funcs.Add(f);

            foreach (KeyValuePair<string, HLSLSource> kv in allIncludes)
            {
                foreach (HLSLFunction fun in kv.Value.methods)
                {
                    funcs.Add(fun);
                }
            }
        }

    }
}
