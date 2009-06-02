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

namespace Babel
{
    /* HLSLFunction
     * Data struct that represents a function that has been declared in HLSL. 
     * Stores name, description, type, and parameters.
     * Also can recover the entire declaration (including parameters).
     */
	public struct HLSLFunction
	{
		public string Name;
		public string Description;
		public string Type;
		public IList<HLSLParameter> Parameters;

        override public string ToString()
        {
            string str = Type + " " + Name + "(";
            for (int i = 0; i < Parameters.Count - 1; i++)
            {
                str += Parameters[i].Name + ", ";
            }
            if (Parameters.Count > 0)
            {
                str += Parameters[Parameters.Count - 1].Name;
            }
            str += ")";
            return str;
        }
	}

    /*HLSLParameter
     * Stores name, display value, and description of each parameter.
     */
	public struct HLSLParameter
	{
		public string Name;
		public string Display;
		public string Description;
	}
}