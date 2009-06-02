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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Babel
{
    public class HLSLAuthoringSink
    {
        private AuthoringSink sink;

        //constructor
        public HLSLAuthoringSink(AuthoringSink sink)
        {
            this.sink = sink;
        }

        //set end parameters to context
        public virtual void EndParameters(TextSpan context)
        {
            sink.EndParameters(context);
        }

        //set next parameters to context
        public virtual void NextParameter(TextSpan context)
        {
            sink.NextParameter(context);
        }

        //set start name to name at span
        public virtual void StartName(TextSpan span, string name)
        {
            sink.StartName(span, name);
        }

        //set start parameters to context
        public virtual void StartParameters(TextSpan context)
        {
            sink.StartParameters(context);
        }
    }
}
