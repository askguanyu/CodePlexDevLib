//-----------------------------------------------------------------------
// <copyright file="ModernDataGridViewHelper.cs" company="YuGuan Corporation">
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
        /// Field _isHorizontal.
        /// </summary>
        private bool _isHorizontal = false;

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
        /// <param name="vertical">Whether use vertical.</param>
        public ModernDataGridViewHelper(ModernScrollBar scrollBar, DataGridView dataGridView, bool vertical = true)
        {
            this._scrollBar = scrollBar;
            this._scrollBar.UseBarColor = true;
            this._dataGridView = dataGridView;
            this._isHorizontal = !vertical;

            foreach (var item in this._dataGridView.Controls)
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

            this._dataGridView.RowsAdded += this.OnDataGridViewRowsAdded;
            this._dataGridView.UserDeletedRow += this.OnDataGridViewUserDeletedRow;
            this._dataGridView.Scroll += this.OnDataGridViewScroll;
            this._dataGridView.Resize += this.OnDataGridViewResize;
            this._scrollBar.Scroll += this.OnScrollBarScroll;
            this._scrollBar.ScrollbarSize = 17;

            this.UpdateScrollBar();
        }

        /// <summary>
        /// Updates the ScrollBar values.
        /// </summary>
        public void UpdateScrollBar()
        {
            try
            {
                this.BeginIgnoreScrollbarChangeEvents();

                if (this._isHorizontal)
                {
                    int visibleCols = this.GetVisibleColumns();

                    this._scrollBar.Maximum = this._hScrollBar.Maximum;
                    this._scrollBar.Minimum = this._hScrollBar.Minimum;
                    this._scrollBar.SmallChange = this._hScrollBar.SmallChange;
                    this._scrollBar.LargeChange = this._hScrollBar.LargeChange;
                    this._scrollBar.Location = new Point(0, this._dataGridView.Height - this._scrollBar.ScrollbarSize);
                    this._scrollBar.Width = this._dataGridView.Width - (this._vScrollBar.Visible ? this._scrollBar.ScrollbarSize : 0);
                    this._scrollBar.BringToFront();
                    this._scrollBar.Visible = this._hScrollBar.Visible;
                    this._scrollBar.Value = this._hScrollBar.Value == 0 ? 1 : this._hScrollBar.Value;
                }
                else
                {
                    int visibleRows = this.GetVisibleRows();

                    this._scrollBar.Maximum = this._dataGridView.RowCount;
                    this._scrollBar.Minimum = 1;
                    this._scrollBar.SmallChange = 1;
                    this._scrollBar.LargeChange = Math.Max(1, visibleRows - 1);
                    this._scrollBar.Value = this._dataGridView.FirstDisplayedScrollingRowIndex;
                    this._scrollBar.Location = new Point(this._dataGridView.Width - this._scrollBar.ScrollbarSize, 0);
                    this._scrollBar.Height = this._dataGridView.Height - (this._hScrollBar.Visible ? this._scrollBar.ScrollbarSize : 0);
                    this._scrollBar.BringToFront();
                    this._scrollBar.Visible = this._vScrollBar.Visible;
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
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void OnDataGridViewScroll(object sender, ScrollEventArgs e)
        {
            this.UpdateScrollBar();
        }

        /// <summary>
        /// Handles the UserDeletedRow event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewRowEventArgs"/> instance containing the event data.</param>
        private void OnDataGridViewUserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            this.UpdateScrollBar();
        }

        /// <summary>
        /// Handles the RowsAdded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewRowsAddedEventArgs"/> instance containing the event data.</param>
        private void OnDataGridViewRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            this.UpdateScrollBar();
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

            if (this._isHorizontal)
            {
                this._hScrollBar.Value = this._scrollBar.Value;

                try
                {
                    this._dataGridView.HorizontalScrollingOffset = this._scrollBar.Value;
                }
                catch
                {
                }
            }
            else
            {
                if (this._scrollBar.Value >= 0 && this._scrollBar.Value < this._dataGridView.Rows.Count)
                {
                    this._dataGridView.FirstDisplayedScrollingRowIndex = this._scrollBar.Value + (this._scrollBar.Value == 1 ? -1 : 1);
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

        /// <summary>
        /// Handles the Resize event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDataGridViewResize(object sender, EventArgs e)
        {
            this.UpdateScrollBar();
        }
    }
}
