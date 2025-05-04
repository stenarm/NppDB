// In INppDBCommandHost.cs

using System;

namespace NppDB.Comm
{
    public interface INppDbCommandHost
    {
        object Execute(NppDbCommandType type, object[] parameters);
        // Keep SendNppMessage ONLY if it's used elsewhere, otherwise remove.
        // Let's assume it might be useful later and keep it for now.
        void SendNppMessage(uint Msg, IntPtr wParam, int lParam);
        // REMOVE: void SetCurrentSciLanguage(string lexerName);
    }
}