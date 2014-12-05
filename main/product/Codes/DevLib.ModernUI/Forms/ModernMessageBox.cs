//-----------------------------------------------------------------------
// <copyright file="ModernMessageBox.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Media;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Modern-styled message notification.
    /// </summary>
    public static class ModernMessageBox
    {
        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text)
        {
            return Show(null, text, "Notification");
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(IWin32Window owner, string text)
        {
            return Show(owner, text, "Notification");
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption)
        {
            return Show(null, text, caption, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            return Show(owner, text, caption, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return Show(null, text, caption, buttons, MessageBoxIcon.None);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return Show(owner, text, caption, buttons, MessageBoxIcon.None);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <param name="defaultbutton">One of the System.Windows.Forms.MessageBoxDefaultButton values that specifies the default button for the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultbutton)
        {
            return Show(null, text, caption, buttons, icon, defaultbutton);
        }

        /// <summary>
        /// Shows a modern-styles message notification into the specified owner window.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <param name="defaultbutton">One of the System.Windows.Forms.MessageBoxDefaultButton values that specifies the default button for the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultbutton)
        {
            DialogResult result = DialogResult.None;

            Form ownerForm = null;

            if (owner != null)
            {
                ownerForm = (Form)owner;
            }
            else
            {
                ownerForm = Form.ActiveForm;
            }

            if (ownerForm == null)
            {
                return result;
            }

            switch (icon)
            {
                case MessageBoxIcon.Error:
                    SystemSounds.Hand.Play();
                    break;
                case MessageBoxIcon.Exclamation:
                    SystemSounds.Exclamation.Play();
                    break;
                case MessageBoxIcon.Question:
                    SystemSounds.Beep.Play();
                    break;
                default:
                    SystemSounds.Asterisk.Play();
                    break;
            }

            ModernMessageBoxForm modernMessageBoxControl = new ModernMessageBoxForm();
            modernMessageBoxControl.BackColor = ownerForm.BackColor;
            modernMessageBoxControl.Properties.Buttons = buttons;
            modernMessageBoxControl.Properties.DefaultButton = defaultbutton;
            modernMessageBoxControl.Properties.Icon = icon;
            modernMessageBoxControl.Properties.Text = text;
            modernMessageBoxControl.Properties.Caption = caption;
            modernMessageBoxControl.Padding = new Padding(0, 0, 0, 0);
            modernMessageBoxControl.ControlBox = false;
            modernMessageBoxControl.ShowInTaskbar = false;
            modernMessageBoxControl.Size = new Size(ownerForm.Size.Width, modernMessageBoxControl.Height);
            modernMessageBoxControl.Location = new Point(ownerForm.Location.X, ownerForm.Location.Y + ((ownerForm.Height - modernMessageBoxControl.Height) / 2));
            modernMessageBoxControl.ArrangeAppearance();
            int overlaySizes = Convert.ToInt32(Math.Floor(modernMessageBoxControl.Size.Height * 0.28));

            modernMessageBoxControl.ShowDialog();
            modernMessageBoxControl.BringToFront();
            modernMessageBoxControl.SetDefaultButton();

            Action<ModernMessageBoxForm> action = new Action<ModernMessageBoxForm>(ModalState);
            IAsyncResult asyncResult = action.BeginInvoke(modernMessageBoxControl, null, action);
            bool cancelled = false;

            try
            {
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents();
                }
            }
            catch
            {
                cancelled = true;

                if (!asyncResult.IsCompleted)
                {
                    try
                    {
                        asyncResult = null;
                    }
                    catch
                    {
                    }
                }

                action = null;
            }

            if (!cancelled)
            {
                result = modernMessageBoxControl.Result;
                modernMessageBoxControl.Dispose();
                modernMessageBoxControl = null;
            }

            return result;
        }

        /// <summary>
        /// ModalState method.
        /// </summary>
        /// <param name="control">ModernMessageBoxControl instance.</param>
        private static void ModalState(ModernMessageBoxForm control)
        {
            while (control.Visible)
            {
            }
        }
    }
}