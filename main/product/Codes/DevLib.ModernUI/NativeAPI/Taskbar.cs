//-----------------------------------------------------------------------
// <copyright file="Taskbar.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.NativeAPI
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;

    internal enum TaskbarPosition
    {
        Unknown = -1,
        Left,
        Top,
        Right,
        Bottom,
    }

    internal class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Rectangle Bounds
        {
            get;
            private set;
        }

        public TaskbarPosition Position
        {
            get;
            private set;
        }

        public Point Location
        {
            get
            {
                return this.Bounds.Location;
            }
        }

        public Size Size
        {
            get
            {
                return this.Bounds.Size;
            }
        }

        public bool AlwaysOnTop
        {
            get;
            private set;
        }

        public bool AutoHide
        {
            get;
            private set;
        }

        [SecuritySafeCritical]
        public Taskbar()
        {
            IntPtr taskbarHandle = WinApi.FindWindow(Taskbar.ClassName, null);
            WinApi.APPBARDATA data = new WinApi.APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(WinApi.APPBARDATA));
            data.hWnd = taskbarHandle;
            IntPtr result = WinApi.SHAppBarMessage(WinApi.ABM.GetTaskbarPos, ref data);

            if (result == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            this.Position = (TaskbarPosition)data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.Left, data.rc.Top, data.rc.Right, data.rc.Bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(WinApi.APPBARDATA));
            result = WinApi.SHAppBarMessage(WinApi.ABM.GetState, ref data);
            int state = result.ToInt32();

            this.AlwaysOnTop = (state & WinApi.AlwaysOnTop) == WinApi.AlwaysOnTop;
            this.AutoHide = (state & WinApi.Autohide) == WinApi.Autohide;
        }
    }
}