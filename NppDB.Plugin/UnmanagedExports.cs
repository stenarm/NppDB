// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.

using System;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppPlugin.DllExport;

namespace NppDB //Kbg.NppPluginNET
{
    static class UnmanagedExports
    {
        private static readonly NppDbPlugin _plugin = new NppDbPlugin();

        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return NppDbPlugin.IsUnicode();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            _plugin.SetInfo(notepadPlusData);
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            return NppDbPlugin.GetFuncsArray(ref nbF);
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint message, UIntPtr wParam, IntPtr lParam)
        {
            return Convert.ToUInt32(_plugin.MessageProc(message));
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            return _plugin.getName();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            var notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
            _plugin.beNotified(notification);            
        }
    }
}
