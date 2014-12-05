//-----------------------------------------------------------------------
// <copyright file="WinCaret.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class WinCaret
    {
        [DllImport("User32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, int hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        private static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        private static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool HideCaret(IntPtr hWnd);

        private IntPtr _controlHandle;

        public WinCaret(IntPtr ownerHandle)
        {
            this._controlHandle = ownerHandle;
        }

        public bool Create(int width, int height)
        {
            return CreateCaret(this._controlHandle, 0, width, height);
        }

        public void Hide()
        {
            HideCaret(this._controlHandle);
        }

        public void Show()
        {
            ShowCaret(this._controlHandle);
        }

        public bool SetPosition(int x, int y)
        {
            return SetCaretPos(x, y);
        }

        public void Destroy()
        {
            DestroyCaret();
        }
    }
}