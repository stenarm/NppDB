// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Kbg.NppPluginNET.PluginInfrastructure
{
    public abstract class Win32
    {
        /// <summary>
        /// Get the scroll information of a scroll bar or window with scroll bar
        /// @see https://msdn.microsoft.com/en-us/library/windows/desktop/bb787537(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ScrollInfo
        {
            /// <summary>
            /// Specifies the size, in bytes, of this structure. The caller must set this to sizeof(SCROLLINFO).
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// Specifies the scroll bar parameters to set or retrieve.
            /// @see ScrollInfoMask
            /// </summary>
            public uint fMask;
            /// <summary>
            /// Specifies the minimum scrolling position.
            /// </summary>
            public int nMin;
            /// <summary>
            /// Specifies the maximum scrolling position.
            /// </summary>
            public int nMax;
            /// <summary>
            /// Specifies the page size, in device units. A scroll bar uses this value to determine the appropriate size of the proportional scroll box.
            /// </summary>
            public uint nPage;
            /// <summary>
            /// Specifies the position of the scroll box.
            /// </summary>
            public int nPos;
            /// <summary>
            /// Specifies the immediate position of a scroll box that the user is dragging. 
            /// An application can retrieve this value while processing the SB_THUMBTRACK request code. 
            /// An application cannot set the immediate scroll position; the SetScrollInfo function ignores this member.
            /// </summary>
            public int nTrackPos;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

        /// <summary>
        /// Used for the ScrollInfo fMask
        /// SIF_ALL             => Combination of SIF_PAGE, SIF_POS, SIF_RANGE, and SIF_TRACKPOS.
        /// SIF_DISABLENOSCROLL => This value is used only when setting a scroll bar's parameters. If the scroll bar's new parameters make the scroll bar unnecessary, disable the scroll bar instead of removing it.
        /// SIF_PAGE            => The nPage member contains the page size for a proportional scroll bar.
        /// SIF_POS             => The nPos member contains the scroll box position, which is not updated while the user drags the scroll box.
        /// SIF_RANGE           => The nMin and nMax members contain the minimum and maximum values for the scrolling range.
        /// SIF_TRACKPOS        => The nTrackPos member contains the current position of the scroll box while the user is dragging it.
        /// </summary>
        public enum ScrollInfoMask
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS
        }

        /// <summary>
        /// Used for the GetScrollInfo() nBar parameter
        /// </summary>
        public enum ScrollInfoBar
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }
        
        public static readonly IntPtr HwndTop = new IntPtr(0);

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            ASYNC_WINDOW_POS = 0x4000,
            DEFER_ERASE = 0x2000,
            DRAW_FRAME = 0x0020,
            FRAME_CHANGED = 0x0020,
            HIDE_WINDOW = 0x0080,
            NO_ACTIVATE = 0x0010,
            NO_COPY_BITS = 0x0100,
            NO_MOVE = 0x0002,
            NO_OWNER_Z_ORDER = 0x0200,
            NO_REDRAW = 0x0008,
            NO_REPOSITION = 0x0200,
            NO_SEND_CHANGING = 0x0400,
            NO_RESIZE = 0x0001,
            NO_Z_ORDER = 0x0004,
            SHOW_WINDOW = 0x0040,
        }

        public enum Wm : uint
        {
            MOVE = 0x0003,
            SIZE = 0x0005,
            MOVING = 0x0216,
            ENTER_SIZE_MOVE = 0x0231,
            EXIT_SIZE_MOVE = 0x0232,
            NOTIFY = 0x4E,
            COMMAND = 0x0111,
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        [DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        [DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        [DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        [DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, out IntPtr lParam);

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, NppMenuCmd lParam)
        {
            return SendMessage(hWnd, msg, new IntPtr(wParam), new IntPtr((uint)lParam));
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam)
        {
            return SendMessage(hWnd, msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, out int lParam)
        {
            var retval = SendMessage(hWnd, msg, new IntPtr(wParam), out var outVal);
            lParam = outVal.ToInt32();
            return retval;
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, int lParam)
        {
            return SendMessage(hWnd, msg, wParam, new IntPtr(lParam));
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam)
        {
            return SendMessage(hWnd, msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam)
        {
            return SendMessage(hWnd, msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, IntPtr wParam, int lParam)
        {
            return SendMessage(hWnd, (uint)msg, wParam, new IntPtr(lParam));
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, (uint)msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, string lParam)
        {
            return SendMessage(hWnd, (uint)msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam)
        {
            return SendMessage(hWnd, (uint)msg, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, int wParam, int lParam)
        {
            return SendMessage(hWnd, (uint)msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as 
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.  
        /// If gateways are missing or incomplete, please help extend them and send your code to the project 
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, SciMsg msg, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, (uint)msg, wParam, lParam);
        }

        /// <summary>
        /// You should try to avoid calling this method in your plugin code. Rather use one of the gateways such as
        /// <see cref="ScintillaGateway"/> or <see cref="NotepadPPGateway"/>.
        /// If gateways are missing or incomplete, please help extend them and send your code to the project
        /// at https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, ref LangType lParam)
        {
            var retval = SendMessage(hWnd, msg, new IntPtr(wParam), out var outVal);
            lParam = (LangType)outVal;
            return retval;
        }
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public const int MaxPath = 260;

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [MarshalAs(UnmanagedType.LPArray)] byte[] lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32")]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        
        

        public const int MfBycommand = 0;
        public const int MfChecked = 8;
        public const int MfUnchecked = 0;
        public const int MfEnabled = 0;
        public const int MfGrayed = 1;
        public const int MfDisabled = 2;

        [DllImport("user32")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32")]
        public static extern int CheckMenuItem(IntPtr hMenu, int uIdCheckItem, int uCheck);

        [DllImport("user32")]
        public static extern bool EnableMenuItem(IntPtr hMenu, int uIdEnableItem, int uEnable);

        public const int WmCreate = 1;

        [DllImport("user32")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("kernel32")]
        public static extern void OutputDebugString(string lpOutputString);

        /// <summary>
        /// @see https://msdn.microsoft.com/en-us/library/windows/desktop/bb787583(v=vs.85).aspx
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="nBar"></param>
        /// <param name="scrollInfo"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int GetScrollInfo(IntPtr hwnd, int nBar, ref ScrollInfo scrollInfo);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);

        [DllImport("kernel32")]
        public static extern void SetStdHandle(uint nStdHandle, IntPtr handle);

        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
    }
}
