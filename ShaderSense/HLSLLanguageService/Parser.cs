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
using Microsoft.VisualStudio.TextManager.Interop;
using Babel.ParserGenerator;
using Microsoft.VisualStudio.Package;
using System.Collections;

namespace Babel.Parser
{
    /* Parser (partial class)
     * Contains miniture data-classes.
     * Also contains code for determining scope of data, and whether the variable/function/member
     * is relevant in the specified scope.
     */
    public partial class Parser
    {
        const int GLYPHBASE = 6;
        const int GLYPHSTRUCT = GLYPHBASE * 18;
        const int GLYPHTYPEDEF = GLYPHBASE * 20;
        const int GLYPHVARIABLE = GLYPHBASE * 23;
        public const int GLYPH_TYPE_FUNCTION = GLYPHBASE * 12;

        public static IList<Babel.HLSLFunction> methods = new List<Babel.HLSLFunction>();
        private static List<HLSLDeclaration> tempMembers = new List<HLSLDeclaration>();
        private static CodeScope tempCurScope = null;
        private static CodeScope tempLastScope = null;
        private static Dictionary<string, VarDecl> tempFunctionVars = new Dictionary<string, VarDecl>();
        public static Dictionary<string, VarDecl> globalVars = new Dictionary<string, VarDecl>();
        public static List<HLSLDeclaration> structDecls = new List<HLSLDeclaration>();
        public static Dictionary<string, StructMembers> structMembers = new Dictionary<string, StructMembers>();
        public static List<HLSLDeclaration> typedefTypes = new List<HLSLDeclaration>();
        public static CodeScope programScope;
        public static Dictionary<TextSpan, string> identNamesLocs = new Dictionary<TextSpan, string>();
        public static Dictionary<TextSpan, string> funcNamesLocs = new Dictionary<TextSpan, string>();
        private static Dictionary<TextSpan, KeyValuePair<TextSpan, LexValue>> forLoopVars = new Dictionary<TextSpan, KeyValuePair<TextSpan, LexValue>>();
        public static Dictionary<TextSpan, LexValue> structVars = new Dictionary<TextSpan, LexValue>();

        //used to represent a variable declaration
        public class VarDecl
        {
            public HLSLDeclaration varDeclaration;
            public TextSpan varLocation;

            public VarDecl(HLSLDeclaration decl, TextSpan loc)
            {
                varDeclaration = decl;
                varLocation = loc;
            }
        }

        //used to represent a code scope
        public class CodeScope
        {
            public List<CodeScope> innerScopes;
            public Dictionary<string, VarDecl> scopeVars;
            public TextSpan scopeLocation;
            public CodeScope outer;

            public CodeScope(Dictionary<string, VarDecl> vars, TextSpan loc)
            {
                innerScopes = new List<CodeScope>();
                scopeVars = new Dictionary<string, VarDecl>(vars);
                scopeLocation = loc;
                outer = null;
            }

            public CodeScope(TextSpan loc)
            {
                innerScopes = new List<CodeScope>();
                scopeVars = new Dictionary<string,VarDecl>();
                scopeLocation = loc;
                outer = null;
            }
        }

        //used to store members of a struct
        public class StructMembers
        {
            public string structName;
            public List<HLSLDeclaration> structMembers;

            public StructMembers(string name, List<HLSLDeclaration> members)
            {
                structName = name;
                structMembers = new List<HLSLDeclaration>(members);
            }
        }

        public static void PrepareParse(TextSpan programLoc)
        {
            programScope = new CodeScope(programLoc);
            tempCurScope = programScope;
        }

        public void BeginScope(LexLocation loc)
        {
            CodeScope scope = new CodeScope(MkTSpan(loc));
            scope.outer = tempCurScope;
            tempCurScope.innerScopes.Add(scope);
            tempCurScope = scope;
        }

        public void EndScope(LexLocation loc)
        {
            tempCurScope.scopeLocation = TextSpanHelper.Merge(tempCurScope.scopeLocation, MkTSpan(loc));

            Dictionary<TextSpan, KeyValuePair<TextSpan, LexValue>> deferred = new Dictionary<TextSpan, KeyValuePair<TextSpan, LexValue>>();
            foreach (KeyValuePair<TextSpan, KeyValuePair<TextSpan, LexValue>> kv in forLoopVars)
            {
                //If the for loop isn't embedded in this scope, then continue to defer it
                if (TextSpanHelper.IsEmbedded(TextSpanHelper.Merge(kv.Value.Key, kv.Key), tempCurScope.scopeLocation))
                    CheckForLoopScope(kv.Value.Value, kv.Value.Key, kv.Key);
                else
                    deferred.Add(kv.Key, new KeyValuePair<TextSpan, LexValue>(kv.Value.Key, kv.Value.Value));
            }
            forLoopVars.Clear();
            forLoopVars = new Dictionary<TextSpan, KeyValuePair<TextSpan, LexValue>>(deferred);

            tempLastScope = tempCurScope;
            tempCurScope = tempCurScope.outer;

            
        }

        //Records a variable declaration that will later get added to a scope
        public void AddVariable(LexValue varName, LexValue type, LexLocation loc)
        {
            if (!shouldAddDeclarations())
            {
                return;
            }
            if (varName.str != null)
            {
                HLSLDeclaration newDecl = new Babel.HLSLDeclaration(type.str, varName.str, GLYPHVARIABLE, varName.str);

                tempCurScope.scopeVars.Add(varName.str, new VarDecl(newDecl, MkTSpan(loc)));
            }
        }

        public void AddFunctionParamVar(LexValue varName, LexValue type, LexLocation loc)
        {
            HLSLDeclaration newDecl = new HLSLDeclaration(type.str, varName.str, GLYPHVARIABLE, varName.str);
            tempFunctionVars.Add(varName.str, new VarDecl(newDecl, MkTSpan(loc)));
        }

        //Adds a struct type and its members
        public void AddStructType(LexValue loc)
        {
            if (!shouldAddDeclarations())
            {
                return;
            }
            HLSLDeclaration structDecl = new HLSLDeclaration("struct", loc.str, GLYPHSTRUCT, loc.str);
            structDecls.Add(structDecl);
            structMembers.Add(loc.str, new StructMembers(loc.str, tempMembers));
            tempMembers.Clear();
        }

        //Adds a typedef'ed type
        public void AddTypedefType(LexValue type, LexValue newType)
        {
            if (!shouldAddDeclarations())
            {
                return;
            }
            typedefTypes.Add(new HLSLDeclaration(type.str, newType.str, GLYPHTYPEDEF, newType.str));
        }

        //Creates a list of member variables that are within a struct
        public void AddStructMember(LexValue type, LexValue identifier)
        {
            tempMembers.Add(new HLSLDeclaration(type.str, identifier.str, GLYPHVARIABLE, identifier.str));
        }

        // Add function to list of autocompletions, eventually method completion also
        public void AddFunction(LexValue type, LexValue name, LexValue parameters)
        {
            if (!shouldAddDeclarations() || parameters.str == null)
            {
                return;
            }
            HLSLFunction method = new HLSLFunction();
            method.Name = name.str;
            method.Type = type.str;
            method.Parameters = new List<HLSLParameter>();
            if (parameters.str != "")
            {
                string[] splitParams = parameters.str.Split(',');
                foreach (string param in splitParams)
                {
                    HLSLParameter parameter = new HLSLParameter();
                    parameter.Description = param;
                    parameter.Name = param;
                    parameter.Display = param;
                    method.Parameters.Add(parameter);
                }
            }
            methods.Add(method);

            foreach (KeyValuePair<string, VarDecl> kv in tempFunctionVars)
                tempLastScope.scopeVars.Add(kv.Key, kv.Value);

            tempFunctionVars.Clear();
        }

        //Used by the parser to combine multiple tokens' string values into a single token
        public LexValue Lexify(string strToLex)
        {
            LexValue val = new LexValue();
            val.str = strToLex;
            return val;

        }

        //Called before the new parse starts; clears the current lists
        public static void clearDeclarations()
        {
            Parser.structDecls.Clear();
            Parser.structMembers.Clear();
            Parser.typedefTypes.Clear();
            Parser.methods.Clear();
            Parser.programScope = null;
            Parser.globalVars.Clear();
            Parser.forLoopVars.Clear();
            Parser.structVars.Clear();
            Parser.identNamesLocs.Clear();
            Parser.funcNamesLocs.Clear();
        }

        //Determines whether the parser should add declarations or not
        public bool shouldAddDeclarations()
        {
            return request.Reason == ParseReason.Check
                || request.Reason == ParseReason.CompleteWord
                || request.Reason == ParseReason.DisplayMemberList
                || request.Reason == ParseReason.MemberSelect
                || request.Reason == ParseReason.MemberSelectAndHighlightBraces
                || request.Reason == ParseReason.MethodTip
                || request.Reason == ParseReason.QuickInfo;
        }

        public void AddIdentifierToCheck(LexValue identifier, LexLocation idenLoc)
        {
            identNamesLocs.Add(MkTSpan(idenLoc), identifier.str);
        }

        public void MarkIdentifierAsFunction(LexValue identifier, LexLocation idenLoc)
        {
            if(identNamesLocs.ContainsKey(MkTSpan(idenLoc)))
                identNamesLocs.Remove(MkTSpan(idenLoc));

            funcNamesLocs.Add(MkTSpan(idenLoc), identifier.str);
        }

        //Need to defer the checking of the for loop scope until the scope it is in finishes parsing
        public void DeferCheckForLoopScope(LexValue assignVal, LexLocation forHeader, LexLocation forBody)
        {
            forLoopVars.Add(MkTSpan(forBody), new KeyValuePair<TextSpan, LexValue>(MkTSpan(forHeader), assignVal));
        }

        public void CheckForLoopScope(LexValue assignVal, TextSpan forHeader, TextSpan forBody)
        {
            if (assignVal.str != null)
            {
                if (!(assignVal.str.Equals(string.Empty)))
                {
                    CodeScope forscope;
                    string[] typeAndName = assignVal.str.Split(' ');
                    if (HLSLScopeUtils.HasScopeForSpan(forBody, programScope, out forscope))
                    {
                        forscope.scopeVars.Add(typeAndName[1], new VarDecl(new HLSLDeclaration(typeAndName[0], typeAndName[1], GLYPHVARIABLE, typeAndName[1]), forBody));
                        forscope.scopeLocation = TextSpanHelper.Merge(forHeader, forBody);
                    }
                    else
                    {
                        TextSpan forScopeLocation = TextSpanHelper.Merge(forHeader, forBody);
                        CodeScope forCs = new CodeScope(new Dictionary<string, VarDecl>(), forScopeLocation);
                        forCs.outer = forscope;
                        forCs.scopeVars.Add(typeAndName[1], new VarDecl(new HLSLDeclaration(typeAndName[0], typeAndName[1], GLYPHVARIABLE, typeAndName[1]), forBody));

                        if (forscope.innerScopes.Count == 0)
                        {
                            forscope.innerScopes.Add(forCs);
                        }
                        else
                        {
                            bool inserted = false;
                            for (int i = 0; i < forscope.innerScopes.Count; i++)
                            {
                                if (TextSpanHelper.EndsBeforeStartOf(forScopeLocation, forscope.innerScopes[i].scopeLocation))
                                {
                                    forscope.innerScopes.Insert(i, forCs);
                                    inserted = true;
                                    break;
                                }
                            }
                            if (!inserted)
                                forscope.innerScopes.Add(forCs);
                        }
                    }
                }
            }
        }

        //Used for locating the struct var that precedes the dot that triggered a MemberSelect operation
        public void AddStructVarForCompletion(LexValue varName, LexLocation loc)
        {
            structVars.Add(MkTSpan(loc), varName);
        }
    }
}
