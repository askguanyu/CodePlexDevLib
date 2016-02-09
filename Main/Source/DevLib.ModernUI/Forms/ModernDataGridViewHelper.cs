//-----------------------------------------------------------------------
// <copyright file="ModernDataGridViewHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// ModernDataGridViewHelper class.
    /// </summary>
    public class ModernDataGridViewHelper
    {
        /// <summary>
        /// Field _scrollBar.
        /// </summary>
        private ModernScrollBar _scrollBar;

        /// <summary>
        /// Field _dataGridView.
        /// </summary>
        private DataGridView _dataGridView;

        /// <summary>
        /// Field _ignoreScrollBarChange.
        /// </summary>
        private int _ignoreScrollBarChange = 0;

        /// <summary>
        /// Field _isVertical.
        /// </summary>
        private bool _isVertical = true;

        /// <summary>
        /// Field _hScrollBar.
        /// </summary>
        private HScrollBar _hScrollBar = null;

        /// <summary>
        /// Field _vScrollBar.
        /// </summary>
        private VScrollBar _vScrollBar = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernDataGridViewHelper"/> class.
        /// </summary>
        /// <param name="scrollBar">The ModernScrollBar.</param>
        /// <param name="dataGridView">The DataGridView.</param>
        /// <param name="isVertical">Whether use vertical.</param>
        public ModernDataGridViewHelper(ModernScrollBar scrollBar, DataGridView dataGridView, bool isVertical)
        {
            this._scrollBar = scrollBar;
            this._scrollBar.UseBarColor = true;
            this._dataGridView = dataGridView;
            this._isVertical = isVertical;

            foreach (object item in this._dataGridView.Controls)
            {
                if (item.GetType() == typeof(VScrollBar))
                {
                    this._vScrollBar = (VScrollBar)item;
                }

                if (item.GetType() == typeof(HScrollBar))
                {
                    this._hScrollBar = (HScrollBar)item;
                }
            }

            this._scrollBar.Scroll += this.OnScrollBarScroll;

            this.UpdateScrollBar();
        }

        /// <summary>
        /// Gets or sets the size of the corner.
        /// </summary>
        public int CornerSize
        {
            get;
            set;
        }

        /// <summary>
        /// Updates the ScrollBar values.
        /// </summary>
        public void UpdateScrollBar()
        {
            try
            {
                this.BeginIgnoreScrollbarChangeEvents();

                if (this._isVertical)
                {
                    this._scrollBar.Maximum = this._dataGridView.RowCount;
                    this._scrollBar.Minimum = 1;
                    this._scrollBar.SmallChange = 1;
                    this._scrollBar.LargeChange = Math.Max(1, this.GetVisibleRows() - 1);
                    this._scrollBar.Value = this._dataGridView.FirstDisplayedScrollingRowIndex;
                    this._scrollBar.Location = new Point(this._dataGridView.Width - this._scrollBar.ScrollbarSize, 0);
                    this._scrollBar.Height = this._dataGridView.Height - (this._hScrollBar.Visible ? this.CornerSize : 0);
                    this._scrollBar.Visible = this._vScrollBar.Visible;
                    this._scrollBar.BringToFront();
                }
                else
                {
                    this._scrollBar.Maximum = this._hScrollBar.Maximum;
                    this._scrollBar.Minimum = this._hScrollBar.Minimum;
                    this._scrollBar.SmallChange = this._hScrollBar.SmallChange;
                    this._scrollBar.LargeChange = this._hScrollBar.LargeChange;
                    this._scrollBar.Value = this._hScrollBar.Value == 0 ? 1 : this._hScrollBar.Value;
                    this._scrollBar.Location = new Point(0, this._dataGridView.Height - this._scrollBar.ScrollbarSize);
                    this._scrollBar.Width = this._dataGridView.Width - (this._vScrollBar.Visible ? this.CornerSize : 0);
                    this._scrollBar.Visible = this._hScrollBar.Visible;
                    this._scrollBar.BringToFront();
                }
            }
            finally
            {
                this.EndIgnoreScrollbarChangeEvents();
            }
        }

        /// <summary>
        /// Gets the vertical scroll visible.
        /// </summary>
        /// <returns>true if visible; otherwise, false.</returns>
        public bool GetVerticalScrollVisible()
        {
            bool result = false;

            if (this._dataGridView.DisplayedRowCount(true) < this._dataGridView.RowCount + (this._dataGridView.RowHeadersVisible ? 1 : 0))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets the horizontal scroll visible.
        /// </summary>
        /// <returns>true if visible; otherwise, false.</returns>
        public bool GetHorizontalScrollVisible()
        {
            bool result = false;

            if (this._dataGridView.DisplayedColumnCount(true) < this._dataGridView.ColumnCount + (this._dataGridView.ColumnHeadersVisible ? 1 : 0))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Handles the Scroll event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ScrollEventArgs"/> instance containing the event data.</param>
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            if (this._ignoreScrollBarChange > 0)
            {
                return;
            }

            if (this._isVertical)
            {
                if (this._scrollBar.Value >= 0 && this._scrollBar.Value < this._dataGridView.Rows.Count)
                {
                    int index = this._scrollBar.Value + (this._scrollBar.Value == 1 ? -1 : 1);

                    if (index < 0)
                    {
                        index = 0;
                    }
                    else if (index >= this._dataGridView.RowCount)
                    {
                        index = this._dataGridView.RowCount - 1;
                    }

                    this._dataGridView.FirstDisplayedScrollingRowIndex = index;
                }
            }
            else
            {
                try
                {
                    this._hScrollBar.Value = this._scrollBar.Value;
                }
                catch
                {
                }

                try
                {
                    this._dataGridView.HorizontalScrollingOffset = this._scrollBar.Value;
                }
                catch
                {
                }
            }

            this._dataGridView.Invalidate();
        }

        /// <summary>
        /// Begins the ignore scrollbar change events.
        /// </summary>
        private void BeginIgnoreScrollbarChangeEvents()
        {
            this._ignoreScrollBarChange++;
        }

        /// <summary>
        /// Ends the ignore scrollbar change events.
        /// </summary>
        private void EndIgnoreScrollbarChangeEvents()
        {
            if (this._ignoreScrollBarChange > 0)
            {
                this._ignoreScrollBarChange--;
            }
        }

        /// <summary>
        /// Returns the number of rows displayed to the user.
        /// </summary>
        /// <returns>The number of rows displayed to the user.</returns>
        private int GetVisibleRows()
        {
            return this._dataGridView.DisplayedRowCount(true);
        }

        /// <summary>
        /// Returns the number of columns displayed to the user.
        /// </summary>
        /// <returns>The number of columns displayed to the user.</returns>
        private int GetVisibleColumns()
        {
            return this._dataGridView.DisplayedColumnCount(true);
        }
    }
}
