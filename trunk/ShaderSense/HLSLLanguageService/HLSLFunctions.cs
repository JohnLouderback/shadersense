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
    /* HLSLFunctions
     * A list of HLSLFunction types. Has getters to read data from members of the list.
     * 
     */
	public class HLSLFunctions : Microsoft.VisualStudio.Package.Methods
	{
		IList<HLSLFunction> methods;
		public HLSLFunctions(IList<HLSLFunction> methods)
		{
			this.methods = methods;
		}

        //get the count
		public override int GetCount()
		{
			return methods.Count;
		}

        //get the name
		public override string GetName(int index)
		{
			return methods[index].Name;
		}

        //get the description
		public override string GetDescription(int index)
		{
			return methods[index].Description;
		}

        //get the type
		public override string GetType(int index)
		{
			return methods[index].Type;
		}

        //get the parameter count
		public override int GetParameterCount(int index)
		{
			return (methods[index].Parameters == null) ? 0 : methods[index].Parameters.Count;
		}

        //get the parameter info
		public override void GetParameterInfo(int index, int paramIndex, out string name, out string display, out string description)
		{
			HLSLParameter parameter = methods[index].Parameters[paramIndex];
			name = parameter.Name;
			display = parameter.Display;
			description = parameter.Description;
		}
	}
}