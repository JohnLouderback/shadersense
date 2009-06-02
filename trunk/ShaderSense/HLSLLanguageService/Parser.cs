﻿using System;
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

        public static List<Babel.HLSLDeclaration> storedVars = new List<Babel.HLSLDeclaration>();
        public static IList<Babel.HLSLFunction> methods = new List<Babel.HLSLFunction>();
        private static List<HLSLDeclaration> tempMembers = new List<HLSLDeclaration>();
        private static List<CodeScope> tempScopes = new List<CodeScope>();
        private static Dictionary<string, VarDecl> tempVars = new Dictionary<string, VarDecl>();
        public static Dictionary<string, VarDecl> globalVars = new Dictionary<string, VarDecl>();
        public static List<HLSLDeclaration> structDecls = new List<HLSLDeclaration>();
        public static List<StructMembers> structMembers = new List<StructMembers>();
        public static List<HLSLDeclaration> typedefTypes = new List<HLSLDeclaration>();
        public static CodeScope programScope;
        public static List<string> identifierNames = new List<string>();
        public static List<TextSpan> identifierLocs = new List<TextSpan>();

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


        // Tries to add inner scopes to the current scope
        public void AddScope(LexLocation loc)
        {
            CodeScope scope = new CodeScope(tempVars, MkTSpan(loc));
            tempVars.Clear();
            foreach (CodeScope cs in tempScopes)
            {
                if (TextSpanHelper.IsEmbedded(cs.scopeLocation, MkTSpan(loc)))
                {
                    scope.innerScopes.Add(cs);
                    cs.outer = scope;
                    tempScopes.Remove(cs);
                }
            }
            tempScopes.Add(scope);
        }

        //Creates the main program scope that includes functions and global variables
        public void AddProgramScope(LexLocation loc)
        {
            programScope = new CodeScope(globalVars, MkTSpan(loc));
            foreach (CodeScope cs in tempScopes)
                cs.outer = programScope;
            programScope.innerScopes.AddRange(tempScopes);
        }

        //Records a variable declaration that will later get added to a scope
        public void AddVariable(LexValue varName, LexValue type, LexLocation loc)
        {
            if (!shouldAddDeclarations())
            {
                return;
            }
            HLSLDeclaration newDecl = new Babel.HLSLDeclaration(type.str, varName.str, GLYPHVARIABLE, varName.str);
            storedVars.Add(newDecl);

            tempVars.Add(varName.str, new VarDecl(newDecl, MkTSpan(loc)));
        }

        //Tells the parser to add the last added variable declaration as a global var
        public void AddVarAsGlobal()
        {
            if (tempVars.Count != 0)
            {
                globalVars.Add(tempVars.ElementAt(0).Key, tempVars.ElementAt(0).Value);
                tempVars.Clear();
            }
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
            structMembers.Add(new StructMembers(loc.str, tempMembers));
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
            Parser.storedVars.Clear();
            Parser.structDecls.Clear();
            Parser.structMembers.Clear();
            Parser.typedefTypes.Clear();
            Parser.methods.Clear();
            Parser.tempScopes.Clear();
            Parser.programScope = null;
            Parser.tempVars.Clear();
            Parser.globalVars.Clear();
            Parser.identifierLocs.Clear();
            Parser.identifierNames.Clear();
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
            identifierNames.Add(identifier.str);
            identifierLocs.Add(MkTSpan(idenLoc));
        }
    }
}
