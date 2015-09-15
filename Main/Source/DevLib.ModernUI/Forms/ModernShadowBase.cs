//-----------------------------------------------------------------------
// <copyright file="ModernShadowBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// ModernShadowBase class.
    /// </summary>
    internal abstract class ModernShadowBase : Form
    {
        /// <summary>
        /// Field WS_EX_TRANSPARENT.
        /// </summary>
        protected const int WS_EX_TRANSPARENT = 0x20;

        /// <summary>
        /// Field WS_EX_LAYERED.
        /// </summary>
        protected const int WS_EX_LAYERED = 0x80000;

        /// <summary>
        /// Field WS_EX_NOACTIVATE.
        /// </summary>
        protected const int WS_EX_NOACTIVATE = 0x8000000;

        /// <summary>
        /// Field TICKS_PER_MS.
        /// </summary>
        private const int TICKS_PER_MS = 10000;

        /// <summary>
        /// Field RESIZE_REDRAW_INTERVAL.
        /// </summary>
        private const long RESIZE_REDRAW_INTERVAL = 1000 * TICKS_PER_MS;

        /// <summary>
        /// Field _shadowSize.
        /// </summary>
        private readonly int _shadowSize;

        /// <summary>
        /// Field _wsExStyle.
        /// </summary>
        private readonly int _wsExStyle;

        /// <summary>
        /// Field _isBringingToFront.
        /// </summary>
        private bool _isBringingToFront;

        /// <summary>
        /// Field _lastResizedOn.
        /// </summary>
        private long _lastResizedOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernShadowBase"/> class.
        /// </summary>
        /// <param name="targetForm">The target form.</param>
        /// <param name="shadowSize">Size of the shadow.</param>
        /// <param name="wsExStyle">The wsExStyle.</param>
        protected ModernShadowBase(Form targetForm, int shadowSize, int wsExStyle)
        {
            this.TargetForm = targetForm;
            this._shadowSize = shadowSize;
            this._wsExStyle = wsExStyle;

            this.TargetForm.Activated += this.OnTargetFormActivated;
            this.TargetForm.ResizeBegin += this.OnTargetFormResizeBegin;
            this.TargetForm.ResizeEnd += this.OnTargetFormResizeEnd;
            this.TargetForm.VisibleChanged += this.OnTargetFormVisibleChanged;
            this.TargetForm.SizeChanged += this.OnTargetFormSizeChanged;
            this.TargetForm.Move += this.OnTargetFormMove;
            this.TargetForm.Resize += this.OnTargetFormResize;

            if (this.TargetForm.Owner != null)
            {
                this.Owner = this.TargetForm.Owner;
            }

            this.TargetForm.Owner = this;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            this.Bounds = this.GetShadowBounds();
        }

        /// <summary>
        /// Gets the target form.
        /// </summary>
        protected Form TargetForm
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the create parameters.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= this._wsExStyle;
                return cp;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is resizing.
        /// </summary>
        private bool IsResizing
        {
            get
            {
                return this._lastResizedOn > 0;
            }
        }

        /// <summary>
        /// Paints the shadow.
        /// </summary>
        protected abstract void PaintShadow();

        /// <summary>
        /// Clears the shadow.
        /// </summary>
        protected abstract void ClearShadow();

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Deactivate" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this._isBringingToFront = true;
        }

        /// <summary>
        /// Gets the shadow bounds.
        /// </summary>
        /// <returns>Rectangle instance.</returns>
        private Rectangle GetShadowBounds()
        {
            Rectangle rectangle = this.TargetForm.Bounds;
            rectangle.Inflate(this._shadowSize, this._shadowSize);
            return rectangle;
        }

        /// <summary>
        /// Called when Activated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormActivated(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Update();
            }

            if (this._isBringingToFront)
            {
                this.Visible = true;
                this._isBringingToFront = false;
                return;
            }

            this.BringToFront();
        }

        /// <summary>
        /// Called when VisibleChanged.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormVisibleChanged(object sender, EventArgs e)
        {
            this.Visible = this.TargetForm.Visible && this.TargetForm.WindowState != FormWindowState.Minimized;
            this.Update();
        }

        /// <summary>
        /// Called when ResizeBegin.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormResizeBegin(object sender, EventArgs e)
        {
            this._lastResizedOn = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Called when Move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormMove(object sender, EventArgs e)
        {
            if (!this.TargetForm.Visible || this.TargetForm.WindowState != FormWindowState.Normal)
            {
                this.Visible = false;
            }
            else
            {
                this.Bounds = this.GetShadowBounds();
            }
        }

        /// <summary>
        /// Called when Resize.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormResize(object sender, EventArgs e)
        {
            this.ClearShadow();
        }

        /// <summary>
        /// Called when SizeChanged.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormSizeChanged(object sender, EventArgs e)
        {
            this.Bounds = this.GetShadowBounds();

            if (this.IsResizing)
            {
                return;
            }

            this.PaintShadowIfVisible();
        }

        /// <summary>
        /// Called when ResizeEnd.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTargetFormResizeEnd(object sender, EventArgs e)
        {
            this._lastResizedOn = 0;
            this.PaintShadowIfVisible();
        }

        /// <summary>
        /// Paints the shadow if visible.
        /// </summary>
        private void PaintShadowIfVisible()
        {
            if (this.TargetForm.Visible && this.TargetForm.WindowState != FormWindowState.Minimized)
            {
                this.PaintShadow();
            }
        }
    }
}
