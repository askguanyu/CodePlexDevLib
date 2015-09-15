//-----------------------------------------------------------------------
// <copyright file="SubClass.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.NativeAPI
{
    using System;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class SubClass : NativeWindow
    {
        public delegate int SubClassWndProcEventHandler(ref Message m);

        public event SubClassWndProcEventHandler SubClassedWndProc;

        public SubClass(IntPtr Handle, bool isSubClass)
        {
            base.AssignHandle(Handle);
            this.SubClassed = isSubClass;
        }

        public bool SubClassed
        {
            get;
            set;
        }

        protected override void WndProc(ref Message m)
        {
            if (this.SubClassed)
            {
                if (this.OnSubClassedWndProc(ref m) != 0)
                {
                    return;
                }
            }

            base.WndProc(ref m);
        }

        public void CallDefaultWndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        public int HiWord(int Number)
        {
            return (Number >> 16) & 0xffff;
        }

        public int LoWord(int Number)
        {
            return Number & 0xffff;
        }

        public int MakeLong(int LoWord, int HiWord)
        {
            return (HiWord << 16) | (LoWord & 0xffff);
        }

        public IntPtr MakeLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        private int OnSubClassedWndProc(ref Message m)
        {
            if (this.SubClassedWndProc != null)
            {
                return this.SubClassedWndProc(ref m);
            }

            return 0;
        }
    }
}
