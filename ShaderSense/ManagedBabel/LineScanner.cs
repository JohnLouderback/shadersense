/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Babel.Parser;
using Microsoft.VisualStudio.Package;

namespace Babel
{
	/// <summary>
	/// LineScanner wraps the GPLEX scanner to provide the IScanner interface
	/// required by the Managed Package Framework. This includes mapping tokens
	/// to color definitions.
	/// </summary>
	public class LineScanner : IScanner
	{
		Babel.ParserGenerator.IColorScan lex = null;

		public LineScanner()
		{
			this.lex = new Babel.Lexer.Scanner();
		}

		public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
		{
			int start, end;
			int token = lex.GetNext(ref state, out start, out end);

			// !EOL and !EOF
			if (token != (int)Tokens.EOF)
			{
                return processToken(tokenInfo, token, start, end);
			}
			else
			{
				return false;
			}
        }

        public void SetSource(string source, int offset)
        {
            lex.SetSource(source, offset);
        }

        /// <summary>
        /// Processes a token for coloring
        /// </summary>
        /// <param name="tokenInfo">TokenInfo object to store info into</param>
        /// <param name="token">The token itself</param>
        /// <param name="start">Start index of token</param>
        /// <param name="end">End index of token</param>
        /// <returns>True on success, false otherwise</returns>
        private bool processToken(TokenInfo tokenInfo, int token, int start, int end)
        {
            Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
            tokenInfo.StartIndex = start;
            tokenInfo.EndIndex = end;
            tokenInfo.Color = definition.TokenColor;
            tokenInfo.Type = definition.TokenType;
            tokenInfo.Trigger = definition.TokenTriggers;
            return true;
        }
	}
}