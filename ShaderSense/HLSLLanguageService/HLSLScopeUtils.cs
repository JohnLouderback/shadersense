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
using Babel.Parser;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{
    class HLSLScopeUtils
    {
        //get the current scope of the cursor
        public static Parser.CodeScope GetCurrentScope(int line, int col)
        {
            return GetCurrentScope(Parser.Parser.programScope, line, col);
        }

        public static Parser.CodeScope GetCurrentScope(Parser.CodeScope codeScope, int line, int col)
        {
            foreach (Parser.CodeScope cs in codeScope.innerScopes)
            {
                if (TextSpanHelper.ContainsExclusive(cs.scopeLocation, line, col))
                {
                    Parser.CodeScope recursive = GetCurrentScope(cs, line, col);
                    if (recursive == null)
                        return cs;
                    else
                        return recursive;
                }
            }

            return codeScope;
        }

        //gets the variables that are valid within the given scope
        public static void GetVarDecls(Parser.CodeScope scope, Dictionary<string, Parser.VarDecl> varDecls)
        {
            foreach (KeyValuePair<string, Parser.VarDecl> vd in scope.scopeVars)
                varDecls.Add(vd.Key, vd.Value);

            if (scope.outer != null)
                GetVarDecls(scope.outer, varDecls);
        }

        //If it returns true, scope contains the scope that has the matching TextSpan
        //If it returns false, scope contains the scope that TextSpan ts would be contained in
        public static bool HasScopeForSpan(TextSpan ts, Parser.CodeScope globalScope, out Parser.CodeScope scope)
        {
            foreach (Parser.CodeScope cs in globalScope.innerScopes)
            {
                if (TextSpanHelper.IsSameSpan(ts, cs.scopeLocation))
                {
                    scope = cs;
                    return true;
                }
                else
                {
                    if(TextSpanHelper.IsEmbedded(ts, cs.scopeLocation))
                    {
                        return HasScopeForSpan(ts, cs, out scope);
                    }
                }
            }
            scope = globalScope;
            return false;
        }
    }
}
