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
    /* HLSLDeclarations 
     * Stores a list of HLSLDeclaration variables. Has getters for the list.
     */
	public class HLSLDeclarations : Microsoft.VisualStudio.Package.Declarations
	{
		IList<HLSLDeclaration> declarations;
		public HLSLDeclarations(IList<HLSLDeclaration> declarations)
		{
			this.declarations = declarations;
		}

        //get the count
		public override int GetCount()
		{
			return declarations.Count;
		}

        //get the description
		public override string GetDescription(int index)
		{
			return declarations[index].Description;
		}

        //get the display text
		public override string GetDisplayText(int index)
		{
			return declarations[index].DisplayText;
		}

        //get the glyph
		public override int GetGlyph(int index)
		{
			return declarations[index].Glyph;
		}

        //get the name
		public override string GetName(int index)
		{
			if (index >= 0)
				return declarations[index].Name;

			return null;
		}
	}
}