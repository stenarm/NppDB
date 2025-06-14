﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.

using System;
using System.Runtime.InteropServices;

namespace NppPlugin.DllExport
{
    [AttributeUsage(AttributeTargets.Method)]
    class DllExportAttribute : Attribute
    {
        public CallingConvention CallingConvention { get; set; }
        public string ExportName { get; set; }
        
        public DllExportAttribute()
        {
        }

        public DllExportAttribute(string exportName) : this(exportName, CallingConvention.StdCall)
        {
        }

        public DllExportAttribute(string exportName, CallingConvention callingConvention)
        {
            ExportName = exportName;
            CallingConvention = callingConvention;
        }
    }
}
