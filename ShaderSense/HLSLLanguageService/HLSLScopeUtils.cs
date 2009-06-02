using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Babel.Parser;
using Microsoft.VisualStudio.Package;

namespace Babel
{
    class HLSLScopeUtils
    {
        //get the current scope of the cursor
        public static Parser.Parser.CodeScope GetCurrentScope(Parser.Parser.CodeScope codeScope, int line, int col)
        {
            foreach (Parser.Parser.CodeScope cs in codeScope.innerScopes)
            {
                if (TextSpanHelper.ContainsExclusive(cs.scopeLocation, line, col))
                {
                    Parser.Parser.CodeScope recursive = GetCurrentScope(cs, line, col);
                    if (recursive == null)
                        return cs;
                    else
                        return recursive;
                }
            }

            return null;
        }

        //gets the variables that are valid within the given scope
        public static void GetVarDecls(Parser.Parser.CodeScope scope, Dictionary<string, Parser.Parser.VarDecl> varDecls)
        {
            foreach (KeyValuePair<string, Parser.Parser.VarDecl> vd in scope.scopeVars)
                varDecls.Add(vd.Key, vd.Value);

            if (scope.outer != null)
                GetVarDecls(scope.outer, varDecls);
        }
    }
}
