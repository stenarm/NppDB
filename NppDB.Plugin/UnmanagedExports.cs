using System;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppPlugin.DllExport;

namespace NppDB
{
    internal static class UnmanagedExports
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
        static uint messageProc(uint message, IntPtr wParam, IntPtr lParam)
        {
            return _plugin.MessageProc(message, wParam, lParam);
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            return _plugin.GetName();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            if (notifyCode == IntPtr.Zero)
                return;

            var notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
            _plugin.BeNotified(notification);
        }
    }
}