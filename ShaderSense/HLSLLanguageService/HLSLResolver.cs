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
using System.Text;
using System.IO;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Babel.Parser;
using System.Collections;

namespace Babel
{
    /* HLSLResolver
     * Uses the data from the most recent parse to determine what sorts of values match various criteria.
     * For example, FindCompletions returns all values that might be used as a completion.
     */
	public class HLSLResolver : Babel.IASTResolver
	{
		#region IASTResolver Members
        public HLSLSource _source;

        private static string[] keywordTypes = { "int", "bool", "float", "half", "double", "uint", 
                                                   "buffer", "vector", "matrix", "texture", "sampler", "void" };

        private static string[] preprocessorTokens = { "#define", "#elif", "#else", "#endif", "#error", "#if",
                                                         "#ifdef", "#ifndef", "#include", "#line", "#undef" };

        private bool ShouldAddDecls(string text, int line, int col, Dictionary<string, StructMembers> structTypes)
        {
            if (_source == null)
                return false;

            string lineText = _source.GetText(line, 0, line, col);
            lineText = lineText.Trim();
            if (lineText.Length > 0 && lineText[lineText.Length - 1] == ';')
                return true;

            char[] splitchars = { ' ', '\t', '(', ',', ';' };
            string[] tokens = lineText.Split(splitchars);
            if (tokens.Length < 2)
                return true;

            string prevToken = tokens[tokens.Length - 2]; //-1 gives cur token, -2 gives prev token
            bool isType = false;
            foreach (string s in keywordTypes)
            {
                if (prevToken.StartsWith(s))
                {
                    isType = true;
                    break;
                }
            }
            if (!isType)
            {
                //foreach (HLSLDeclaration decl in Parser.Parser.structDecls)
                //foreach(KeyValuePair<string, Parser.StructMembers> kv in Parser.Parser.structDecls)
                //foreach (KeyValuePair<string, Parser.StructMembers> kv in _source.structDecls)
                foreach (KeyValuePair<string, Parser.StructMembers> kv in structTypes)
                {
                    //if (prevToken.StartsWith(decl.Name))
                    if(prevToken.StartsWith(kv.Key))
                    {
                        isType = true;
                        break;
                    }
                }
            }
            if (!isType)
            {
                //foreach (HLSLDeclaration decl in Parser.Parser.typedefTypes)
                foreach (HLSLDeclaration decl in _source.typedefTypes)
                {
                    if (prevToken.StartsWith(decl.Name))
                    {
                        isType = true;
                        break;
                    }
                }
            }

            if (!isType && (Char.IsLetter(text, 0) || text[0].Equals('_')))
                return true;
            else
                return false;
        }

        //Gets a list of things to put in the auto-completion list, including keywords, intrinsics,
        //variables, types, and functions
		public IList<Babel.HLSLDeclaration> FindCompletions(object result, int line, int col)
        {
            //  Preparation
            List<Babel.HLSLDeclaration> declarations = new List<Babel.HLSLDeclaration>();
            string currentText = result.ToString();

            /*if (currentText.Equals("#"))
            {
                foreach (string s in preprocessorTokens)
                {
                    declarations.Add(new HLSLDeclaration(s, s, 6 * 9, s));
                }

                return declarations;
            }*/

            Dictionary<string, StructMembers> structTypes = new Dictionary<string, StructMembers>();
            _source.GatherStructDecls(structTypes);

            if (!ShouldAddDecls(currentText, line, col, structTypes))
                return declarations;


            bool isGlobalScope = false;
            Parser.CodeScope scope = HLSLScopeUtils.GetCurrentScope(_source.programScope, line, col);
            if (scope == _source.programScope)
                isGlobalScope = true;

            //  Adding predefined keyword commands
            foreach (string command in Babel.Lexer.Scanner.Commands)
            {
                int glyph = 206;
                //if (currentText == string.Empty || command.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                if (!isGlobalScope)
                {
                    foreach (string s in keywordTypes)
                    {
                        if (command.StartsWith(s))
                        {
                            glyph = 6 * 21;
                            break;
                        }
                    }
                    declarations.Add(new Babel.HLSLDeclaration(Babel.Lexer.Scanner.GetDescriptionForTokenValue(command), command, glyph, command));
                }
                else
                {
                    foreach (string s in keywordTypes)
                    {
                        if(command.StartsWith(s))
                            declarations.Add(new Babel.HLSLDeclaration(Babel.Lexer.Scanner.GetDescriptionForTokenValue(command), command, 6 * 21, command));
                    }
                }
            }
            
            if (!isGlobalScope)
            {
                // Add predefined intrinsics
                foreach (string intrin in Babel.Lexer.Scanner.Intrinsics)
                {
                    //if (currentText == string.Empty || intrin.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        declarations.Add(new Babel.HLSLDeclaration(Babel.Lexer.Scanner.GetDescriptionForTokenValue(intrin), intrin, 6 * 25, intrin));
                    }
                }

                //  Add function declarations
                //foreach (HLSLFunction method in Parser.Parser.methods)
                List<HLSLFunction> allFuncs = new List<HLSLFunction>();
                _source.GatherFunctions(allFuncs);
                foreach (HLSLFunction method in allFuncs)
                {
                    //if (currentText == string.Empty || method.Name.StartsWith(currentText, true, null))
                    {
                        declarations.Add(methodToDeclaration(method));
                    }
                }

                //  Add variable declarations
                Dictionary<string, Parser.VarDecl> vars = new Dictionary<string, Babel.Parser.VarDecl>();
                _source.GatherVariables(scope, vars);
                //HLSLScopeUtils.GetVarDecls(scope, vars);

                //Add the variables to the list
                foreach (KeyValuePair<string, Parser.VarDecl> kv in vars)
                {
                    //if (currentText == string.Empty || kv.Key.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                    declarations.Add(kv.Value.varDeclaration);
                }
            }

            //  Add struct declarations
            //foreach (HLSLDeclaration d in Parser.Parser.structDecls)
            //foreach(KeyValuePair<string, Parser.StructMembers> kv in Parser.Parser.structDecls)
            //foreach (KeyValuePair<string, Parser.StructMembers> kv in _source.structDecls)
            foreach (KeyValuePair<string, Parser.StructMembers> kv in structTypes)
            {
                //if (currentText == string.Empty || d.Name.StartsWith(currentText))
                {
                    declarations.Add(kv.Value.structDecl);
                }
            }

            //  Add type definitions
            //foreach (HLSLDeclaration d in Parser.Parser.typedefTypes)
            foreach (HLSLDeclaration d in _source.typedefTypes)
            {
                //if (currentText == string.Empty || d.Name.StartsWith(currentText))
                {
                    declarations.Add(d);
                }
            }

            //  Sort the declarations
            declarationComparer dc = new declarationComparer();
            declarations.Sort(dc);

            return declarations;
        }

        //a comparer for comparing the names in two declarations
        private class declarationComparer : IComparer<HLSLDeclaration>
        {
            public int Compare(HLSLDeclaration a, HLSLDeclaration b)
            {
                return a.Name.CompareTo(b.Name);
            }

        }

        public void FindMembersWrapper(string tempDir, int line, int col)
        {
            string oldDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(tempDir);

            FindMembers(null, line, col);

            Directory.SetCurrentDirectory(oldDir);
        }

        //Gets a list of member variables of the struct variable that came before the dot 
        //at line 'line' and column 'col'
		public IList<Babel.HLSLDeclaration> FindMembers(object result, int line, int col)
		{
            string TestFileOutputName = "MemberCompleteOutput.txt";
            FileStream fw = new FileStream(TestFileOutputName, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fw);

			// ManagedMyC.Parser.AAST aast = result as ManagedMyC.Parser.AAST;
			List<Babel.HLSLDeclaration> members = new List<Babel.HLSLDeclaration>();

            //string currentText = result.ToString();

            //foreach (string state in aast.startStates.Keys)
            //    members.Add(new Declaration(state, state, 0, state));

            //int start, end;
            string token = null;
            TokenInfo[] info = _source.GetColorizer().GetLineInfo(_source.GetTextLines(), line, _source.ColorState);
            IEnumerator enumer = info.GetEnumerator();
            TokenInfo ticur = null;
            TokenInfo tiprev = null;
            bool foundDot = false;
            if (enumer.MoveNext())
            {
                ticur = (TokenInfo)enumer.Current;
                while (enumer.MoveNext())
                {
                    tiprev = ticur;
                    ticur = (TokenInfo)enumer.Current;
                    if (ticur.Trigger == TokenTriggers.MemberSelect && ticur.Type == TokenType.Operator)
                    {
                        foundDot = true;
                        break;
                    }
                }
            }
            //bool retval = _source.GetWordExtent(line, col >=3 ? col - 3 : col, WORDEXTFLAGS.WORDEXT_PREVIOUS, out start, out end);
            if (foundDot && tiprev != null)
            {
                token = _source.GetText(line, tiprev.StartIndex, line, tiprev.EndIndex + 1);
            }
            else
            {
                KeyValuePair<TextSpan, Parser.LexValue> var = new KeyValuePair<TextSpan, Babel.Parser.LexValue>(new TextSpan(), new Babel.Parser.LexValue());
                //foreach (KeyValuePair<TextSpan, Parser.LexValue> kv in Parser.Parser.structVars)
                foreach (KeyValuePair<TextSpan, Parser.LexValue> kv in _source.structVars)
                {
                    if (TextSpanHelper.IsAfterEndOf(kv.Key, line, col) && TextSpanHelper.EndsAfterEndOf(kv.Key, var.Key))
                        var = kv;
                }
                token = var.Value.str;
            }

            string varType = null;
            if( token != null )
            {
                Dictionary<string, Parser.VarDecl> vars = new Dictionary<string, Babel.Parser.VarDecl>();
                //Parser.CodeScope curCS = HLSLScopeUtils.GetCurrentScope(Parser.Parser.programScope, line, col);
                Parser.CodeScope curCS = HLSLScopeUtils.GetCurrentScope(_source.programScope, line, col);
                //if (curCS == null)
                    //curCS = Parser.Parser.programScope;
                //    curCS = _source.programScope;
                HLSLScopeUtils.GetVarDecls(curCS, vars);
                foreach (KeyValuePair<string, Parser.VarDecl> kv in vars)
                {
                    if (kv.Key.Equals(token))
                    {
                        varType = kv.Value.varDeclaration.Description;
                        break;
                    }
                }
            }

            if (varType != null)
            {
                Parser.StructMembers sm;
                //if (Parser.Parser.structDecls.TryGetValue(varType, out sm))
                Dictionary<string, StructMembers> structTypes = new Dictionary<string, StructMembers>();
                _source.GatherStructDecls(structTypes);
                //if (_source.structDecls.TryGetValue(varType, out sm))
                if (structTypes.TryGetValue(varType, out sm))
                    members.AddRange(sm.structMembers);
            }

            foreach(HLSLDeclaration h in members)
            {
                sw.WriteLine(h.DisplayText);
            }

            sw.Close();
			return members;
		}

        //find guick info  right now returns nothing (unimplemented)
		public string FindQuickInfo(object result, int line, int col)
		{
			return "unknown";
		}

        //Find the method with the given name at the location
		public IList<HLSLFunction> FindMethods(object result, int line, int col, string name)
		{
            List<HLSLFunction> matchingMethods = new List<HLSLFunction>();
            List<HLSLFunction> allFuncs = new List<HLSLFunction>();
            _source.GatherFunctions(allFuncs);
            //foreach (HLSLFunction method in Parser.Parser.methods)
            foreach (HLSLFunction method in allFuncs)
            {
                if (method.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    matchingMethods.Add(method);
                }
            }
			return matchingMethods;
		}

        //convert a method to a declaration
        public HLSLDeclaration methodToDeclaration(Babel.HLSLFunction method)
        {
            HLSLDeclaration dec = new HLSLDeclaration();
            dec.Name = method.Name;
            dec.Glyph = Parser.Parser.GLYPH_TYPE_FUNCTION;
            dec.DisplayText = method.Name;
            dec.Description = method.ToString();
            return dec;
        }

		#endregion
	}
}
