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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Babel.Parser;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{
    /* HLSLLineScanner
     * Processes tokens to determine information about them - stores things like thier start and end values.
     * Also sets flags if tokens are important values - such as tokens representing a parameter list or a member selection.
     */
    public class HLSLLineScanner : IScanner
    {
        //Babel.ParserGenerator.IColorScan lex = null;
        Babel.Lexer.Scanner lex = null;
        HLSLLanguageService _service;
        private const char memberSelectChar = '.';
        private const char paramStartChar = '(';
        private const char paramNextChar = ',';
        private const char paramEndChar = ')';
#if !NOTTEST
        string testFileOutput;
#endif

        public HLSLLineScanner(HLSLLanguageService service)
        {
            this.lex = new Babel.Lexer.Scanner();
            _service = service;
#if !NOTTEST           
            //get the current working directory, we will need to reset this late
            string resetDir = Directory.GetCurrentDirectory();

            //get into the ShaderSense folder
            while (Directory.GetCurrentDirectory().Contains("ShaderSense"))
            {
                Directory.SetCurrentDirectory((Directory.GetParent(Directory.GetCurrentDirectory()).ToString()));
            }

            Directory.SetCurrentDirectory("ShaderSense");

            //this is out output file
            testFileOutput = Directory.GetCurrentDirectory() + "\\TestOutput.txt";

            //clean the file so we can output new data
            if (File.Exists("tooltipsOutput.txt"))
            {
                File.Delete("tooltipsOutput.txt");
            }
            if (File.Exists("tokensfile.txt"))
            {
                RunTooltipTest("tokensfile.txt", "tooltipsOutput.txt");
            }

            Directory.SetCurrentDirectory(resetDir);
#endif
            //System.Windows.Forms.MessageBox.Show("File is: " + testFileOutput);           
        }

        //run the tooltip tests
        public bool RunTooltipTest(String TestFilename,String TestFileoutputName)
        {
            //file output objects
            FileStream fr = new FileStream(TestFilename, FileMode.Open);
            StreamReader sr = new StreamReader(fr);
            FileStream fw = new FileStream(TestFileoutputName, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fw);
            String token;

            //run over each of the tokens
            do
            {
                token = sr.ReadLine();
                if (token != null)
                {
                    sw.WriteLine("{0} {1}", token, Babel.Lexer.Scanner.GetDescriptionForTokenValue(token));
                }
            } while (token != null);

            //clean up
            sw.Close();
            sr.Close();

            return true;
        }

        //scan the token and get info about it
        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            int start, end;
            int token = lex.GetNext(ref state, out start, out end);
            string tok = lex.yytext;

            // !EOL and !EOF
            if (token != (int)Tokens.EOF)
            {
                bool retVal = processToken(tokenInfo, token, start, end);
                if (token == (int)memberSelectChar)
                {
                    tokenInfo.Trigger = TokenTriggers.MemberSelect;
                }
                else if (token == (int)paramStartChar)
                {
                    tokenInfo.Trigger = TokenTriggers.ParameterStart | TokenTriggers.MatchBraces;
                }
                else if (token == (int)paramNextChar)
                {
                    tokenInfo.Trigger = TokenTriggers.ParameterNext;
                }
                else if (token == (int)paramEndChar)
                {
                    tokenInfo.Trigger = TokenTriggers.ParameterEnd | TokenTriggers.MatchBraces;
                }
                else if (token == (int)Tokens.IDENTIFIER)
                {
                    tokenInfo.Trigger |= TokenTriggers.MemberSelect;
                }
                else if (token == (int)Tokens.INTRINSIC)
                {
                    tokenInfo.Trigger |= TokenTriggers.MemberSelect;
                }
                else if (token >= (int)Tokens.KWBLENDSTATE && token <= (int)Tokens.RWVIRTUAL)
                {
                    tokenInfo.Trigger |= TokenTriggers.MemberSelect;
                }
                return retVal;
            }
            else
            {
                return false;
            }
        }

        //Set the source for the code
        public void SetSource(string source, int offset)
        {
            lex.SetSource(source, offset);
            lex.SetCurrentSource((Babel.HLSLSource)(_service.GetSource(_service.LastActiveTextView)));
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
#if !NOTTEST
            FileStream f = new FileStream(testFileOutput, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(f);
#endif

            Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
            tokenInfo.StartIndex = start;
            tokenInfo.EndIndex = end;
            tokenInfo.Color = definition.TokenColor;
            tokenInfo.Type = definition.TokenType;
            tokenInfo.Trigger = definition.TokenTriggers;

#if !NOTTEST
            //write these stuff to an output file for testing
            sw.WriteLine(tokenInfo.Type + " " + tokenInfo.Color);
            sw.Close();
#endif
            return true;
        }
    }
}
