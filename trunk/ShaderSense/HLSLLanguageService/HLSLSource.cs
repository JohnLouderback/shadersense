/**************************************************
 * 
 * Copyright 2009 Garrett Kiel
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
using Microsoft.VisualStudio.Package;
using Babel.Parser;

namespace Babel
{
    public class HLSLSource : Source
    {
        CodeScope programScope;

        public IList<Babel.HLSLFunction> methods;
        //TODO: Don't forget to change the reference in lexer.lex
        public List<HLSLDeclaration> structDecls;
        public List<HLSLDeclaration> typedefTypes;
        public Dictionary<TextSpan, string> identNamesLocs;
        public Dictionary<TextSpan, string> funcNamesLocs;
        public Dictionary<TextSpan, LexValue> structVars;

        public HLSLSource(HLSLLanguageService service, IVsTextLines textLines, Colorizer colorizer)
			: base(service, textLines, colorizer)
		{
		}

        public void PrepareParse(TextSpan programLoc)
        {
            programScope = new CodeScope(programLoc);
            methods = new List<Babel.HLSLFunction>();
            structDecls = new List<HLSLDeclaration>();
            typedefTypes = new List<HLSLDeclaration>();
            identNamesLocs = new Dictionary<TextSpan, string>();
            funcNamesLocs = new Dictionary<TextSpan, string>();
            structVars = new Dictionary<TextSpan, LexValue>();
        }
    }
}
