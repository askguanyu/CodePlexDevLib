//-----------------------------------------------------------------------
// <copyright file="LoggingViewerMainForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging.Viewer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// MainForm class.
    /// </summary>
    public partial class LoggingViewerMainForm : Form
    {
        /// <summary>
        /// Field _isMaximized.
        /// </summary>
        private bool _isMaximized;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingViewerMainForm"/> class.
        /// </summary>
        public LoggingViewerMainForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the SizeChanged event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MainFormSizeChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Maximized:
                    this._isMaximized = true;
                    break;

                case FormWindowState.Minimized:
                    this.ShowInTaskbar = false;
                    this.Hide();
                    break;

                case FormWindowState.Normal:
                    this._isMaximized = false;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the DoubleClick event of the NotifyIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                this.ShowInTaskbar = true;
                this.Show();
                this.WindowState = this._isMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
            }
        }
    }
}
