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
using Microsoft.VisualStudio.Package;
using Babel.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{   
    /* Configuration class.
     * A partial class.
     * This part contains the configuration method, which assigns colors to all the various tokens.
     * Also assigns expected characters for comment tokens and colors for generic tokens (eg, numbers).
     */
	public partial class Configuration
	{
        public const string Name = "HLSL";
        public const string Extension = ".fx";
        public const string FormatList = "HLSL File (*.fx)\n*.fx";

        public static TokenColor opsColor;
        public static TokenColor singleQuoteColor;
        public static TokenColor errorColor;
        public static TokenColor intrinColor;
        public static TokenColor ppColor;
        public static TokenColor structIdentColor;

        static CommentInfo myCInfo;
        public static CommentInfo MyCommentInfo { get { return myCInfo; } }

        //set configurations
        static Configuration()
        {
            myCInfo.BlockEnd = "*/";
            myCInfo.BlockStart = "/*";
            myCInfo.LineStart = "//";
            myCInfo.UseLineComments = true;

            // default colors - currently, these need to be declared
            CreateColor("Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            //CreateColor("Number", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

            opsColor = CreateColor("Operator", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK, false, false);
			singleQuoteColor = CreateColor("SingleQuote", COLORINDEX.CI_RED, COLORINDEX.CI_SYSTEXT_BK, true, false);
            errorColor = CreateColor("Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, true);
            intrinColor = CreateColor("Intrinsic", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, false, false);
            ppColor = CreateColor("Preprocessor", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK, false, false);
            structIdentColor = CreateColor("StructIdent", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, false, false);

            //
            // map tokens to color classes
            //
            ColorToken((int)Tokens.NUMBER, TokenType.Text, TokenColor.Number, TokenTriggers.None);

			ColorToken((int)Tokens.STRING, TokenType.String, TokenColor.String, TokenTriggers.None);
			ColorToken((int)'"', TokenType.Operator, singleQuoteColor, TokenTriggers.None);

            ColorToken((int)Tokens.INTRINSIC, TokenType.Text, intrinColor, TokenTriggers.MethodTip);
            for (int i = (int)Tokens.PPDEFINE; i <= (int)Tokens.PPUNDEF; i++)
            {
                ColorToken(i, TokenType.Text, ppColor, TokenTriggers.None);
            }

            ColorToken((int)Tokens.STRUCTIDENTIFIER, TokenType.String, structIdentColor, TokenTriggers.None);

            ColorToken((int)Tokens.PPINCLFILE, TokenType.Text, TokenColor.String, TokenTriggers.None);
            //  
            //  Our ShaderSense tokens
            // KEYWORDS
            for (int i = (int)Tokens.KWBLENDSTATE; i <= (int)Tokens.RWVIRTUAL; i++)
            {
                ColorToken(i, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            }

            //characters
            ColorToken((int)'(', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            ColorToken((int)')', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            ColorToken((int)'{', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            ColorToken((int)'}', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);

            //  
            //  Our ShaderSense tokens
            // OPERATORS
            for (int i = (int)Tokens.EQ; i <= (int)Tokens.ARROW; i++)
            {
                ColorOperatorToken(i);
            }
            //more characters and operators
            ColorOperatorToken((int)';');
            ColorOperatorToken((int)',');
            ColorOperatorToken((int)'=');
            ColorOperatorToken((int)'^');
            ColorOperatorToken((int)'+');
            ColorOperatorToken((int)'-');
            ColorOperatorToken((int)'*');
            ColorOperatorToken((int)'/');
            ColorOperatorToken((int)'!');
            ColorOperatorToken((int)':');
            ColorOperatorToken((int)'[');
            ColorOperatorToken((int)']');
            ColorOperatorToken((int)'&');
            ColorOperatorToken((int)'|');
            ColorOperatorToken((int)'.');
            ColorOperatorToken((int)'%');


            //// Extra token values internal to the scanner
            ColorToken((int)Tokens.LEX_ERROR, TokenType.Text, errorColor, TokenTriggers.None);
            ColorToken((int)Tokens.LEX_COMMENT, TokenType.Text, TokenColor.Comment, TokenTriggers.None);

        }

        //colors operator tokens
        private static void ColorOperatorToken(int op)
        {
            ColorToken(op, TokenType.Operator, opsColor, TokenTriggers.None);
        }
    }
}