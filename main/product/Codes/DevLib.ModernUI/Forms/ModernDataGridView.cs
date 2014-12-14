//-----------------------------------------------------------------------
// <copyright file="ModernDataGridView.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernDataGridView user control.
    /// </summary>
    public class ModernDataGridView : DataGridView, IModernControl
    {
        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _styleManager.
        /// </summary>
        private ModernStyleManager _styleManager = null;

        /// <summary>
        /// Field _components.
        /// </summary>
        private IContainer _components = null;

        /// <summary>
        /// Field _verticalScrollBar.
        /// </summary>
        private ModernScrollBar _verticalScrollBar;

        /// <summary>
        /// Field _horizontalScrollBar.
        /// </summary>
        private ModernScrollBar _horizontalScrollBar;

        /// <summary>
        /// Field _verticalScrollBarHelper.
        /// </summary>
        private ModernDataGridViewHelper _verticalScrollBarHelper = null;

        /// <summary>
        /// Field _horizontalScrollBarHelper.
        /// </summary>
        private ModernDataGridViewHelper _horizontalScrollBarHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernDataGridView"/> class.
        /// </summary>
        public ModernDataGridView()
        {
            this.InitializeComponent();

            this.HighlightPercentage = 0.2f;

            this.Controls.Add(this._verticalScrollBar);
            this.Controls.Add(this._horizontalScrollBar);

            this.Controls.SetChildIndex(this._verticalScrollBar, 0);
            this.Controls.SetChildIndex(this._horizontalScrollBar, 1);

            this._verticalScrollBar.Visible = false;
            this._horizontalScrollBar.Visible = false;

            this._verticalScrollBarHelper = new ModernDataGridViewHelper(this._verticalScrollBar, this, true);
            this._horizontalScrollBarHelper = new ModernDataGridViewHelper(this._horizontalScrollBar, this, false);

            this.ApplyModernStyle();
        }

        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Category("Modern Appearance")]
        [DefaultValue(ModernColorStyle.Default)]
        public ModernColorStyle ColorStyle
        {
            get
            {
                if (this.DesignMode || this._modernColorStyle != ModernColorStyle.Default)
                {
                    return this._modernColorStyle;
                }

                if (this.StyleManager != null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return this.StyleManager.ColorStyle;
                }

                if (this.StyleManager == null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return ModernColorStyle.Blue;
                }

                return this._modernColorStyle;
            }

            set
            {
                this._modernColorStyle = value;
                this.ApplyModernStyle();
                this.RefreshScrollBarHelper();
            }
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        [Category("Modern Appearance")]
        [DefaultValue(ModernThemeStyle.Default)]
        public ModernThemeStyle ThemeStyle
        {
            get
            {
                if (this.DesignMode || this._modernThemeStyle != ModernThemeStyle.Default)
                {
                    return this._modernThemeStyle;
                }

                if (this.StyleManager != null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return this.StyleManager.ThemeStyle;
                }

                if (this.StyleManager == null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return ModernThemeStyle.Light;
                }

                return this._modernThemeStyle;
            }

            set
            {
                this._modernThemeStyle = value;
                this.ApplyModernStyle();
                this.RefreshScrollBarHelper();
            }
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernStyleManager StyleManager
        {
            get
            {
                return this._styleManager;
            }

            set
            {
                this._styleManager = value;
                this.ApplyModernStyle();
                this.RefreshScrollBarHelper();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom BackColor.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(false)]
        [Category("Modern Behaviour")]
        [DefaultValue(true)]
        public bool UseSelectable
        {
            get
            {
                return this.GetStyle(ControlStyles.Selectable);
            }

            set
            {
                this.SetStyle(ControlStyles.Selectable, value);
            }
        }

        /// <summary>
        /// Gets or sets the highlight percentage.
        /// </summary>
        [DefaultValue(0.2F)]
        public float HighlightPercentage
        {
            get;
            set;
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            this.RefreshScrollBarHelper();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this._components != null))
            {
                this._components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintBackground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintBackground != null)
            {
                this.CustomPaintBackground(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaint" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaint(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaint != null)
            {
                this.CustomPaint(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintForeground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintForeground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintForeground != null)
            {
                this.CustomPaintForeground(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.HandleCreated" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            this.RefreshScrollBarHelper();

            base.OnHandleCreated(e);
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            this.RefreshScrollBarHelper();

            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            this.RefreshScrollBarHelper();

            base.OnVisibleChanged(e);
        }

        /// <summary>
        /// OnMouseWheel method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0 && this.FirstDisplayedScrollingRowIndex > 0)
            {
                try
                {
                    this.FirstDisplayedScrollingRowIndex--;
                }
                catch
                {
                }
            }
            else if (e.Delta < 0)
            {
                try
                {
                    this.FirstDisplayedScrollingRowIndex++;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._horizontalScrollBar = new ModernScrollBar();
            this._verticalScrollBar = new ModernScrollBar();

            ((ISupportInitialize)this).BeginInit();
            this.SuspendLayout();

            this._horizontalScrollBar.LargeChange = 10;
            this._horizontalScrollBar.Location = new Point(0, 0);
            this._horizontalScrollBar.Maximum = 100;
            this._horizontalScrollBar.Minimum = 0;
            this._horizontalScrollBar.MouseWheelBarPartitions = 10;
            this._horizontalScrollBar.Name = "_horizontalScrollBar";
            this._horizontalScrollBar.Orientation = ModernScrollBarOrientation.Horizontal;
            this._horizontalScrollBar.ScrollbarSize = 50;
            this._horizontalScrollBar.Size = new Size(200, 50);
            this._horizontalScrollBar.TabIndex = 0;
            this._horizontalScrollBar.UseSelectable = true;

            this._verticalScrollBar.LargeChange = 10;
            this._verticalScrollBar.Location = new Point(0, 0);
            this._verticalScrollBar.Maximum = 100;
            this._verticalScrollBar.Minimum = 0;
            this._verticalScrollBar.MouseWheelBarPartitions = 10;
            this._verticalScrollBar.Name = "_verticalScrollBar";
            this._verticalScrollBar.Orientation = ModernScrollBarOrientation.Vertical;
            this._verticalScrollBar.ScrollbarSize = 50;
            this._verticalScrollBar.Size = new Size(50, 200);
            this._verticalScrollBar.TabIndex = 0;
            this._verticalScrollBar.UseSelectable = true;

            ((ISupportInitialize)this).EndInit();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Apply modern style.
        /// </summary>
        private void ApplyModernStyle()
        {
            this.BorderStyle = BorderStyle.None;
            this.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.EnableHeadersVisualStyles = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.BackColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            this.BackgroundColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            this.GridColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            this.ForeColor = ModernPaint.ForeColor.Button.Disabled(this.ThemeStyle);
            this.Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel);

            this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.AllowUserToResizeRows = false;
            this.AllowUserToResizeColumns = true;

            this.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.ColumnHeadersDefaultCellStyle.BackColor = ModernPaint.GetStyleColor(this.ColorStyle);
            this.ColumnHeadersDefaultCellStyle.ForeColor = ModernPaint.ForeColor.Button.Press(this.ThemeStyle);

            this.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.RowHeadersDefaultCellStyle.BackColor = ModernPaint.GetStyleColor(this.ColorStyle);
            this.RowHeadersDefaultCellStyle.ForeColor = ModernPaint.ForeColor.Button.Press(this.ThemeStyle);

            this.DefaultCellStyle.BackColor = ModernPaint.BackColor.Form(this.ThemeStyle);

            this.DefaultCellStyle.SelectionBackColor = ControlPaint.Light(ModernPaint.GetStyleColor(this.ColorStyle), this.HighlightPercentage);
            this.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 17, 17);

            this.DefaultCellStyle.SelectionBackColor = ControlPaint.Light(ModernPaint.GetStyleColor(this.ColorStyle), this.HighlightPercentage);
            this.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 17, 17);

            this.RowHeadersDefaultCellStyle.SelectionBackColor = ControlPaint.Light(ModernPaint.GetStyleColor(this.ColorStyle), this.HighlightPercentage);
            this.RowHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 17, 17);

            this.ColumnHeadersDefaultCellStyle.SelectionBackColor = ControlPaint.Light(ModernPaint.GetStyleColor(this.ColorStyle), this.HighlightPercentage);
            this.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 17, 17);
        }

        /// <summary>
        /// Refreshes the scroll bar helper.
        /// </summary>
        private void RefreshScrollBarHelper()
        {
            this._verticalScrollBarHelper.UpdateScrollBar();
            this._horizontalScrollBarHelper.UpdateScrollBar();
        }
    }
}
