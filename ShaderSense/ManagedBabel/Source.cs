/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Babel
{
	public class Source : Microsoft.VisualStudio.Package.Source
	{
		public Source(BabelLanguageService service, IVsTextLines textLines, Colorizer colorizer)
			: base(service, textLines, colorizer)
		{
		}

		private object parseResult;
		public object ParseResult
		{
			get { return parseResult; }
			set { parseResult = value; }
		}

		private IList<TextSpan[]> braces;
		public IList<TextSpan[]> Braces
		{
			get { return braces; }
			set { braces = value; }
		}

        public override CommentInfo GetCommentFormat()
        {
             return Configuration.MyCommentInfo;
        }

        public override void MethodTip(IVsTextView textView, int line, int index, TokenInfo info)
        {
//            BeginParse();
//            ParseResultHandler handler;
//            handler.
            base.MethodTip(textView, line, index, info);
//            BeginParse(line, index, info, ParseReason.MethodTip, textView, new ParseResultHandler(HandleMethodTipResponse)); 
        }
	}
}
