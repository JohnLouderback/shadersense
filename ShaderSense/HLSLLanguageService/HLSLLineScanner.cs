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
        private const char memberSelectChar = '.';
        private const char paramStartChar = '(';
        private const char paramNextChar = ',';
        private const char paramEndChar = ')';
        string testFileOutput;

        public HLSLLineScanner()
        {
            this.lex = new Babel.Lexer.Scanner();
            
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
                    tokenInfo.Trigger = TokenTriggers.ParameterStart;
                }
                else if (token == (int)paramNextChar)
                {
                    tokenInfo.Trigger = TokenTriggers.ParameterNext;
                }
                else if (token == (int)paramEndChar)
                {
                    tokenInfo.Trigger = TokenTriggers.ParameterEnd;
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
            FileStream f = new FileStream(testFileOutput, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(f);

            Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
            tokenInfo.StartIndex = start;
            tokenInfo.EndIndex = end;
            tokenInfo.Color = definition.TokenColor;
            tokenInfo.Type = definition.TokenType;
            tokenInfo.Trigger = definition.TokenTriggers;

            //write these stuff to an output file for testing
            sw.WriteLine(tokenInfo.Type + " " + tokenInfo.Color);
            sw.Close();
            return true;
        }
    }
}
