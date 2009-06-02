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

%using Babel;
%using Babel.Parser;

%namespace Babel.Lexer


%x COMMENT

%{

	public static System.Xml.XmlDocument xKWDoc = GetDoc("keywords.xml");
	public static System.Xml.XmlDocument xIFDoc = GetDoc("intrinsic.xml");
	private static System.Xml.XmlDocument GetDoc(string filename)
	{
		System.Xml.XmlDocument newXDoc = new System.Xml.XmlDocument();
		newXDoc.Load(filename);
		return newXDoc;
    }
	public static System.Xml.XmlNodeList keywords = xKWDoc.GetElementsByTagName("word");
	public static System.Xml.XmlNodeList kwDescriptions = xKWDoc.GetElementsByTagName("description");
	public static System.Xml.XmlNodeList intrWords =  xIFDoc.GetElementsByTagName("word");
	public static System.Xml.XmlNodeList intrDescriptions = xIFDoc.GetElementsByTagName("description");

	public static string[] Commands = CopyXmlListToStrArray(keywords);
	public static string[] CommandDescriptions = CopyXmlListToStrArray(kwDescriptions);
	public static string[] Intrinsics = CopyXmlListToStrArray(intrWords);
	public static string[] IntrinsicDescriptions = CopyXmlListToStrArray(intrDescriptions);
		
	private static string[] CopyXmlListToStrArray(System.Xml.XmlNodeList newXmlList)
	{	
	    string[] newStrArr = new string[newXmlList.Count];
		for(int i=0; i<newXmlList.Count; i++)
		{
			newStrArr[i] = newXmlList[i].InnerText;
		}
		
		return newStrArr;
	}
	
	
	public static string GetDescriptionForTokenValue(string token)
	{
		string command;
		for(int i=0; i<Commands.Length; i++)
		{
			command = Commands[i];
			if((command.ToLower()).Equals(token.ToLower()))
			{
				return CommandDescriptions[i];
			}
		}
	    string intrin;
	    for(int i=0; i<Intrinsics.Length; i++)
	    {
			intrin = Intrinsics[i];
			if((intrin.ToLower()).Equals(token.ToLower()))
			{
				return IntrinsicDescriptions[i];
			}
	    }
	    
	   return string.Empty;
	}
	
	bool IsStructIdent(string text)
	{
		foreach(HLSLDeclaration structIdent in Parser.Parser.structDecls)
	    {
			if( text.Equals(structIdent.Name))
			{
				return true;
			}
	    }
		
		return false;
	}
	
	bool IsIntrinsic(string text)
	{
		foreach( string str in Intrinsics )
		{
			if( str.Equals(text) )
				return true;
		}
		
		return false;
	}
	
    //check if the text fits the type[1-4]x[1-4] format
	bool IsType(string text, string word)
	{
        char[] strChars;
		if(text.StartsWith(word))
		{
			if(text.Length == word.Length)
			{
				return true;
			}
            strChars = text.Remove(0, word.Length).ToCharArray();
            if(IsOneToFour(strChars[0]))
            {
                //type of type1, type2, type3, type4
                if(strChars.Length==1)
                {
                    return true;
                }
                //type of type(1-4)x(1-4)
                else if(strChars.Length==3  && strChars[1].Equals('x')  && IsOneToFour(strChars[2]))
                {                   
                            return true;
                }
            }
		}
		return false;
	}
	
	//check is char is a number 1 through 4
	bool IsOneToFour(char numChar)
	{
		if(numChar.Equals('1') || numChar.Equals('2') || numChar.Equals('3') || numChar.Equals('4'))
		{
			return true;
		}	
		return false;
	}
		
         int GetIdToken(string text)
         {
			//deals with texture, which is case sensitive
			string txt = text.ToLower();
//			if (!txt.ToLower().Equals("texture")) {
//				txt = txt.ToLower();
//			}

			//each case is the beginning letter of the word.  a-z are each case.
            switch (txt[0])
            {
                case 'a':
					if (txt.Equals("auto")) return (int)Tokens.RWAUTO;
					break;
                case 'b':
					if (txt.Equals("blendstate")) return (int)Tokens.KWBLENDSTATE;
					if (IsType(txt,"bool")) return (int)Tokens.KWBOOL;
					if (txt.Equals("break")) return (int)Tokens.KWBREAK;
					if (txt.Equals("buffer")) return (int)Tokens.KWBUFFER;
                    break;
                case 'c':
					if (txt.Equals("cbuffer")) return (int)Tokens.KWCBUFFER;
					if (txt.Equals("centroid")) return (int)Tokens.KWCENTROID;
					if (txt.Equals("compile")) return (int)Tokens.KWCOMPILE;
					if (txt.Equals("const")) return (int)Tokens.KWCONST;
					if (txt.Equals("continue")) return (int)Tokens.KWCONTINUE;
					if (txt.Equals("case")) return (int)Tokens.RWCASE;
					if (txt.Equals("catch")) return (int)Tokens.RWCATCH;
					if (txt.Equals("char")) return (int)Tokens.RWCHAR;
					if (txt.Equals("class")) return (int)Tokens.RWCLASS;
					if (txt.Equals("const_cast")) return (int)Tokens.RWCONSTCAST;
					if (txt.Equals("col_major")) return (int)Tokens.KWCOLMAJOR;
                    break;
                case 'd':
					if (txt.Equals("depthstencilstate")) return (int)Tokens.KWDEPTHSTENCILSTATE;
					if (txt.Equals("depthstencilview")) return (int)Tokens.KWDEPTHSTENCILVIEW;
					if (txt.Equals("discard")) return (int)Tokens.KWDISCARD;
					if (txt.Equals("do")) return (int)Tokens.KWDO;
					if (IsType(txt, "double")) return (int)Tokens.KWDOUBLE;
					if (txt.Equals("default")) return (int)Tokens.RWDEFAULT;
					if (txt.Equals("delete")) return (int)Tokens.RWDELETE;
					if (txt.Equals("dynamic_cast")) return (int)Tokens.RWDYNAMICCAST;
                    break;
                case 'e':
					if (txt.Equals("else")) return (int)Tokens.KWELSE;
					if (txt.Equals("extern")) return (int)Tokens.KWEXTERN;
					if (txt.Equals("enum")) return (int)Tokens.RWENUM;
					if (txt.Equals("explicit")) return (int)Tokens.RWEXPLICIT;
                    break;
                case 'f':
					if (txt.Equals("false")) return (int)Tokens.KWFALSE;
					if (IsType(txt, "float")) return (int)Tokens.KWFLOAT;
					if (txt.Equals("for")) return (int)Tokens.KWFOR;
					if (txt.Equals("friend")) return (int)Tokens.RWFRIEND;
                    break;
                case 'g':
					if (txt.Equals("geometryshader")) return (int)Tokens.KWGEOMETRYSHADER;
					if (txt.Equals("goto")) return (int)Tokens.RWGOTO;
                    break;
                case 'h':
					if (IsType(txt, "half")) return (int)Tokens.KWHALF;   
                    break;
                case 'i':
					if (txt.Equals("if")) return (int)Tokens.KWIF;
					if (text.Equals("in")) return (int)Tokens.KWIN;
					if (txt.Equals("inline")) return (int)Tokens.KWINLINE;
					if (text.Equals("inout")) return (int)Tokens.KWINOUT;
					if (IsType(txt,"int")) return (int)Tokens.KWINT;
                    break;
                case 'l':
					if (text.Equals("linear")) return (int)Tokens.KWLINEAR;
					if (txt.Equals("long")) return (int)Tokens.RWLONG;
                    break;
                case 'm':
					if (txt.Equals("matrix")) return (int)Tokens.KWMATRIX;
					if (txt.Equals("mutable")) return (int)Tokens.RWMUTABLE;
                    break;
                case 'n':
					if (txt.Equals("namespace")) return (int)Tokens.KWNAMESPACE;
					if (txt.Equals("nointerpolation")) return (int)Tokens.KWNOINTERPOLATION;
					if (txt.Equals("noperspective")) return (int)Tokens.KWNOPERSPECTIVE;
					if (txt.Equals("new")) return (int)Tokens.RWNEW;
                    break;
                case 'o':
					if (text.Equals("out")) return (int)Tokens.KWOUT;
					if (txt.Equals("operator")) return (int)Tokens.RWOPERATOR;
                    break;
                case 'p':
					if (txt.Equals("packoffset")) return (int)Tokens.KWPACKOFFSET;
					if (txt.Equals("pass")) return (int)Tokens.KWPASS;
					if (txt.Equals("pixelshader")) return (int)Tokens.KWPIXELSHADER;
					if (txt.Equals("private")) return (int)Tokens.RWPRIVATE;
					if (txt.Equals("protected")) return (int)Tokens.RWPROTECTED;
					if (txt.Equals("public")) return (int)Tokens.RWPUBLIC;
                    break;
                case 'r':
					if (txt.Equals("rasterizerstate")) return (int)Tokens.KWRASTERIZERSTATE;
					if (txt.Equals("rendertargetview")) return (int)Tokens.KWRENDERTARGETVIEW;
					if (txt.Equals("return")) return (int)Tokens.KWRETURN;
					if (txt.Equals("register")) return (int)Tokens.KWREGISTER;
					if (txt.Equals("reinterpret_cast")) return (int)Tokens.RWREINTERPRETCAST;
					if (txt.Equals("row_major")) return (int)Tokens.KWROWMAJOR;
                    break;
                case 's':
					if (txt.Equals("sample")) return (int)Tokens.KWSAMPLE;
					if (txt.Equals("sampler")) return (int)Tokens.KWSAMPLER;
					if (txt.Equals("sampler1d")) return (int)Tokens.KWSAMPLER1D;
					if (txt.Equals("sampler2d")) return (int)Tokens.KWSAMPLER2D;
					if (txt.Equals("sampler3d")) return (int)Tokens.KWSAMPLER3D;
					if (txt.Equals("samplercube")) return (int)Tokens.KWSAMPLERCUBE;
					if (txt.Equals("sampler_state")) return (int)Tokens.KWSAMPLERSTATE;
					if (txt.Equals("samplerstate")) return (int)Tokens.KWD3D10SAMPLERSTATE;
					if (txt.Equals("samplercomparisonstate")) return (int)Tokens.KWSAMPLERCOMPARISONSTATE;
					if (txt.Equals("shared")) return (int)Tokens.KWSHARED;
					if (txt.Equals("stateblock")) return (int)Tokens.KWSTATEBLOCK;
					if (txt.Equals("stateblock_state")) return (int)Tokens.KWSTATEBLOCKSTATE;
					if (txt.Equals("static")) return (int)Tokens.KWSTATIC;
					if (txt.Equals("string")) return (int)Tokens.KWSTRING;
					if (txt.Equals("struct")) return (int)Tokens.KWSTRUCT;
					if (txt.Equals("switch")) return (int)Tokens.KWSWITCH;
					if (txt.Equals("snorm")) return (int)Tokens.KWSNORM;
					if (txt.Equals("short")) return (int)Tokens.RWSHORT;
					if (txt.Equals("signed")) return (int)Tokens.RWSIGNED;
					if (txt.Equals("sizeof")) return (int)Tokens.RWSIZEOF;
					if (txt.Equals("static_cast")) return (int)Tokens.RWSTATICCAST;
                    break;
                case 't':
					if (txt.Equals("tbuffer")) return (int)Tokens.KWTBUFFER;
					if (txt.Equals("technique")) return (int)Tokens.KWTECHNIQUE;
					if (txt.Equals("technique10")) return (int)Tokens.KWTECHNIQUE10;
					if (text.Equals("texture")) return (int)Tokens.KWTEXTURE;
					if (text.Equals("Texture")) return (int)Tokens.KWUPPERTEXTURE;
					if (txt.Equals("texture1d")) return (int)Tokens.KWTEXTURE1D;
					if (txt.Equals("texture1darray")) return (int)Tokens.KWTEXTURE1DARRAY;
					if (txt.Equals("texture2d")) return (int)Tokens.KWTEXTURE2D;
					if (txt.Equals("texture2darray")) return (int)Tokens.KWTEXTURE2DARRAY;
					if (txt.Equals("texture2dms")) return (int)Tokens.KWTEXTURE2DMS;
					if (txt.Equals("texture2dmsarray")) return (int)Tokens.KWTEXTURE2DMSARRAY;
					if (txt.Equals("texture3d")) return (int)Tokens.KWTEXTURE3D;
					if (txt.Equals("texturecube")) return (int)Tokens.KWTEXTURECUBE;
					if (txt.Equals("texturecubearray")) return (int)Tokens.KWTEXTURECUBEARRAY;
					if (txt.Equals("true")) return (int)Tokens.KWTRUE;
					if (txt.Equals("typedef")) return (int)Tokens.KWTYPEDEF;
					if (txt.Equals("template")) return (int)Tokens.RWTEMPLATE;
					if (txt.Equals("this")) return (int)Tokens.RWTHIS;
					if (txt.Equals("throw")) return (int)Tokens.RWTHROW;
					if (txt.Equals("try")) return (int)Tokens.RWTRY;
					if (txt.Equals("typename")) return (int)Tokens.RWTYPENAME;
					break;
                case 'u':
					if (IsType(txt, "uint")) return (int)Tokens.KWUINT;
					if (txt.Equals("uniform")) return (int)Tokens.KWUNIFORM;
					if (txt.Equals("unorm")) return (int)Tokens.KWUNORM;
					if (txt.Equals("union")) return (int)Tokens.RWUNION;
					if (txt.Equals("unsigned")) return (int)Tokens.RWUNSIGNED;
					if (txt.Equals("using")) return (int)Tokens.RWUSING;
					break;
                case 'v':
					if (txt.Equals("vector")) return (int)Tokens.KWVECTOR;
					if (txt.Equals("vertexshader")) return (int)Tokens.KWVERTEXSHADER;
					if (txt.Equals("void")) return (int)Tokens.KWVOID;
					if (txt.Equals("volatile")) return (int)Tokens.KWVOLATILE;
					if (txt.Equals("virtual")) return (int)Tokens.RWVIRTUAL;
                    break;
                case 'w':
					if (txt.Equals("while")) return (int)Tokens.KWWHILE;    
                    break;
                default: 
                    break;
            }
            if( IsIntrinsic(text) )
				return (int)Tokens.INTRINSIC;
			else if (IsStructIdent(text) )
				return (int)Tokens.STRUCTIDENTIFIER;
			else
				return (int)Tokens.IDENTIFIER;
       }
       
       internal void LoadYylval()
       {
           yylval.str = tokTxt;
           yylloc = new LexLocation(tokLin, tokCol, tokLin, tokECol);
       }
       
       public override void yyerror(string s, params object[] a)
       {
           if (handler != null) handler.AddError(s, tokLin, tokCol, tokLin, tokECol);
       }
%}


White0          [ \t\r\f\v]
White           {White0}|\n

CmntStart    \/\*
CmntEnd      \*\/
CmntLine     \/\/
ABStar       [^\*\n]*
CommentAll   [^\n]*
VecMat	([1-4](([xX][1-4])?))?

%%


\<([a-zA-Z0-9\.\\\/])*\>		{ return (int)Tokens.PPINCLFILE; }
\"((\\([abfnrtv\\\"]|[0-7][0-7][0-7]|x[0-9a-fA-F]))|[^\"\\])*\"					{ return (int)Tokens.STRING; }
/*[fF][lL][oO][aA][tT]{VecMat}		{ return (int)Tokens.KWFLOAT; }
[iI][nN][tT]{VecMat}				{ return (int)Tokens.KWINT; }
[bB][oO][oO][lL]{VecMat}			{ return (int)Tokens.KWBOOL; }
[dD][oO][uU][bB][lL][eE]{VecMat}	{ return (int)Tokens.KWDOUBLE; }
[hH][aA][lL][fF]{VecMat}			{ return (int)Tokens.KWHALF; }
[uU][iI][nN][tT]{VecMat}			{ return (int)Tokens.KWUINT; }
*/

[a-zA-Z_][a-zA-Z0-9_]*    { return GetIdToken(yytext); }
[0-9]+([uUlL])?			  { return (int)Tokens.NUMBER; }
0x[0-9a-fA-F]+([uUlL])?	  { return (int)Tokens.NUMBER; }
[0-9]+\.[0-9]*([hHfF])?	  { return (int)Tokens.NUMBER; }
[0-9]*\.[0-9]+([hHfF])?	  { return (int)Tokens.NUMBER; }

;                         { return (int)';';    }
,                         { return (int)',';    }
\(                        { return (int)'(';    }
\)                        { return (int)')';    }
\{                        { return (int)'{';    }
\}                        { return (int)'}';    }
=                         { return (int)'=';    }
\^                        { return (int)'^';    }
\+                        { return (int)'+';    }
\-                        { return (int)'-';    }
\*                        { return (int)'*';    }
\/                        { return (int)'/';    }
\!                        { return (int)'!';    }
:                         { return (int)':';    }
\[                        { return (int)'[';    }
\]                        { return (int)']';    }
==                        { return (int)Tokens.EQ;  }
\!=                       { return (int)Tokens.NEQ;   }
\>                        { return (int)Tokens.GT; }
\>=                       { return (int)Tokens.GTE;    }
\<                        { return (int)Tokens.LT;     }
\<=                       { return (int)Tokens.LTE;    }
\&                        { return (int)'&';    }
\&\&                      { return (int)Tokens.AMPAMP; }
\|                        { return (int)'|';    }
\|\|                      { return (int)Tokens.BARBAR; }
\.                        { return (int)'.';    }
##						  { return (int)Tokens.POUNDPOUND; }
#@						  { return (int)Tokens.POUNDAT; }
\+\+					  { return (int)Tokens.INCR; }
\-\-					  { return (int)Tokens.DECR; }
::						  { return (int)Tokens.SCOPE; }
\<\<					  { return (int)Tokens.LSHIFT; }
\<\<=					  { return (int)Tokens.LSHIFTASSN; }
\>\>					  { return (int)Tokens.RSHIFT; }
\>\>=					  { return (int)Tokens.RSHIFTASSN; }
\.\.\.					  { return (int)Tokens.ELLIPSIS; }
\*=						  { return (int)Tokens.MULTASSN; }
\/=						  { return (int)Tokens.DIVASSN; }
\+=						  { return (int)Tokens.ADDASSN; }
\-=						  { return (int)Tokens.SUBASSN; }
\%						  { return (int)'%'; }
\%=						  { return (int)Tokens.MODASSN; }
\&=						  { return (int)Tokens.ANDASSN; }
\|=						  { return (int)Tokens.ORASSN; }
\^=						  { return (int)Tokens.XORASSN; }
\-\>					  { return (int)Tokens.ARROW; }
\"						  { return (int)'"'; }
\\						  { return (int)'\\'; }
#define					  { return (int)Tokens.PPDEFINE; }
#elif					  { return (int)Tokens.PPELIF; }
#else					  { return (int)Tokens.PPELSE; }
#endif					  { return (int)Tokens.PPENDIF; }
#error					  { return (int)Tokens.PPERROR; }
#if						  { return (int)Tokens.PPIF; }
#ifdef					  { return (int)Tokens.PPIFDEF; }
#ifndef					  { return (int)Tokens.PPIFNDEF; }
#include				  { return (int)Tokens.PPINCLUDE; }
#line					  { return (int)Tokens.PPLINE; }
#undef					  { return (int)Tokens.PPUNDEF; }
#						  { return (int)'#'; }

{CmntStart}{ABStar}\**{CmntEnd} { return (int)Tokens.LEX_COMMENT; } 
{CmntStart}{ABStar}\**          { BEGIN(COMMENT); return (int)Tokens.LEX_COMMENT; }
{CmntLine}{CommentAll}\n		{ return (int)Tokens.LEX_COMMENT; }
<COMMENT>\n                     |                                
<COMMENT>{ABStar}\**            { return (int)Tokens.LEX_COMMENT; }                                
<COMMENT>{ABStar}\**{CmntEnd}   { BEGIN(INITIAL); return (int)Tokens.LEX_COMMENT; }

{White0}+                  { return (int)Tokens.LEX_WHITE; }
\n                         { return (int)Tokens.LEX_WHITE; }
.                          { yyerror("illegal char");
                             return (int)Tokens.LEX_ERROR; }

%{
                      LoadYylval();
%}

%%

/* .... */
