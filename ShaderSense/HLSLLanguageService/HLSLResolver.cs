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
using System.Text;
using System.IO;
using Microsoft.VisualStudio.Package;

namespace Babel
{
    /* HLSLResolver
     * Uses the data from the most recent parse to determine what sorts of values match various criteria.
     * For example, FindCompletions returns all values that might be used as a completion.
     */
	public class HLSLResolver : Babel.IASTResolver
	{
		#region IASTResolver Members
        public Source _source;

        //Gets a list of things to put in the auto-completion list, including keywords, intrinsics,
        //variables, types, and functions
		public IList<Babel.HLSLDeclaration> FindCompletions(object result, int line, int col)
        {
            //  Preparation
            List<Babel.HLSLDeclaration> declarations = new List<Babel.HLSLDeclaration>();
            string currentText = result.ToString();

            //  Adding predefined keyword commands
            foreach (string command in Babel.Lexer.Scanner.Commands)
            {
                if (currentText == string.Empty || command.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                {

                    declarations.Add(new Babel.HLSLDeclaration(Babel.Lexer.Scanner.GetDescriptionForTokenValue(command), command, 0, command));
                }
            }
            // Add predefined intrinsics
            foreach (string intrin in Babel.Lexer.Scanner.Intrinsics)
            {
                if (currentText == string.Empty || intrin.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                {
                    declarations.Add(new Babel.HLSLDeclaration(Babel.Lexer.Scanner.GetDescriptionForTokenValue(intrin), intrin, 6 * 25, intrin));
                }
            }

            //  Add variable declarations
            Parser.Parser.CodeScope curCS = HLSLScopeUtils.GetCurrentScope(Parser.Parser.programScope, line, col);
            if (curCS == null)
                curCS = Parser.Parser.programScope;

            Dictionary<string, Parser.Parser.VarDecl> vars = new Dictionary<string,Babel.Parser.Parser.VarDecl>();
            HLSLScopeUtils.GetVarDecls(curCS, vars);

            //Add the variables to the list
            foreach (KeyValuePair<string, Parser.Parser.VarDecl> kv in vars)
            {
                if (currentText == string.Empty || kv.Key.StartsWith(currentText, StringComparison.CurrentCultureIgnoreCase))
                    declarations.Add(kv.Value.varDeclaration);
            }

            //  Add struct declarations
            foreach (HLSLDeclaration d in Parser.Parser.structDecls)
            {
                if (currentText == string.Empty || d.Name.StartsWith(currentText))
                {
                    declarations.Add(d);
                }
            }

            //  Add type definitions
            foreach (HLSLDeclaration d in Parser.Parser.typedefTypes)
            {
                if (currentText == string.Empty || d.Name.StartsWith(currentText))
                {
                    declarations.Add(d);
                }
            }
           
            //  Add function declarations
            foreach (HLSLFunction method in Parser.Parser.methods)
            {
                if (currentText == string.Empty || method.Name.StartsWith(currentText, true, null))
                {   
                    declarations.Add(methodToDeclaration(method));
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
            TokenInfo info = _source.GetTokenInfo(line, col - 1);
            string token = _source.GetText(line, info.StartIndex, line, info.EndIndex+1);

            string varType = null;
            if( token != null )
            {
                Dictionary<string, Parser.Parser.VarDecl> vars = new Dictionary<string, Babel.Parser.Parser.VarDecl>();
                Parser.Parser.CodeScope curCS = HLSLScopeUtils.GetCurrentScope(Parser.Parser.programScope, line, col);
                if (curCS == null)
                    curCS = Parser.Parser.programScope;
                HLSLScopeUtils.GetVarDecls(curCS, vars);
                foreach (KeyValuePair<string, Parser.Parser.VarDecl> kv in vars)
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
                foreach (Parser.Parser.StructMembers sm in Parser.Parser.structMembers)
                {
                    if (varType.Equals(sm.structName))
                    {
                        members.AddRange(sm.structMembers);
                        break;
                    }
                }
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
		public IList<Babel.HLSLFunction> FindMethods(object result, int line, int col, string name)
		{
            IList<Babel.HLSLFunction> matchingMethods = new List<Babel.HLSLFunction>();
            foreach (HLSLFunction method in Parser.Parser.methods)
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
