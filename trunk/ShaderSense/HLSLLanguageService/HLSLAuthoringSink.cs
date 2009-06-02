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
