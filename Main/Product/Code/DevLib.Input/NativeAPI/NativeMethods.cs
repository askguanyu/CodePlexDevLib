//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Input.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        private const string User32Dll = "User32.dll";
        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        public const uint MOUSEEVENTF_XUP = 0x0100;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const int VKeyShiftMask = 0x0100;
        public const int VKeyCharMask = 0x00FF;
        public const int VK_LBUTTON = 0x0001;
        public const int VK_RBUTTON = 0x0002;
        public const int VK_MBUTTON = 0x0004;
        public const int VK_XBUTTON1 = 0x0005;
        public const int VK_XBUTTON2 = 0x0006;
        public const int SMXvirtualscreen = 76;
        public const int SMYvirtualscreen = 77;
        public const int SMCxvirtualscreen = 78;
        public const int SMCyvirtualscreen = 79;
        public const int MouseeventfVirtualdesk = 0x4000;
        public const int WheelDelta = 120;

        [DllImport(User32Dll)]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport(User32Dll, CharSet = CharSet.Auto)]
        public static extern short VkKeyScan(char ch);

        [DllImport(User32Dll, SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport(User32Dll, ExactSpelling = true, EntryPoint = "GetSystemMetrics", CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        /// <summary>Converts the client-area coordinates of a specified point to screen coordinates.</summary>
        /// <param name="hwndFrom">Handle to the window whose client area is used for the conversion.</param>
        /// <param name="pt">POINT structure that contains the client coordinates to be converted.</param>
        /// <returns>true if the function succeeds, false otherwise.</returns>
        [DllImport(User32Dll, EntryPoint = "ClientToScreen", CharSet = CharSet.Auto)]
        public static extern bool ClientToScreen(IntPtr hwndFrom, [In, Out] ref System.Drawing.Point pt);
    }
}
