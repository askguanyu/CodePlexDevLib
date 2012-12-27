//-----------------------------------------------------------------------
// <copyright file="WinFormRibbon.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.WinForms
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    ///
    /// </summary>
    public partial class WinFormRibbon : Form
    {
        /// <summary>
        ///
        /// </summary>
        public WinFormRibbon()
        {
            this.InitializeComponent();
            this.InitializeRibbonTabContainer();
        }

        #region RibbonLogic
        /// <summary>
        ///
        /// </summary>
        private const int RIBBON_COLLAPSE_HEIGHT = 22;

        /// <summary>
        ///
        /// </summary>
        private const int RIBBON_EXPAND_HEIGHT = 100;

        /// <summary>
        ///
        /// </summary>
        private const int FORM_BORDER_HEIGHT = 60;

        /// <summary>
        ///
        /// </summary>
        private bool _isRibbonTabExpand;

        /// <summary>
        ///
        /// </summary>
        private bool _isRibbonTabShow;

        /// <summary>
        ///
        /// </summary>
        private void InitializeRibbonTabContainer()
        {
            this._isRibbonTabExpand = true;
            this._isRibbonTabShow = true;
            this.CollapseRibbonTabContainer(!this._isRibbonTabExpand);
            this.RibbonTabContainer.LostFocus += this.HideRibbon;
            this.ribbonPageFile.ItemClicked += this.HideRibbon;
            this.ribbonPageHome.ItemClicked += this.HideRibbon;
            this.ribbonPageAbout.ItemClicked += this.HideRibbon;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonTabContainer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.CollapseRibbonTabContainer(this._isRibbonTabExpand);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="whetherCollapse"></param>
        private void CollapseRibbonTabContainer(bool whetherCollapse)
        {
            if (whetherCollapse)
            {
                this.RibbonTabContainer.Height = RIBBON_COLLAPSE_HEIGHT;
                this.RibbonPanel.Location = new System.Drawing.Point(0, RIBBON_COLLAPSE_HEIGHT);
                this.RibbonPanel.Height = this.Height - RIBBON_COLLAPSE_HEIGHT - FORM_BORDER_HEIGHT;
                this._isRibbonTabExpand = false;
                this._isRibbonTabShow = false;
            }
            else
            {
                this.RibbonTabContainer.Height = RIBBON_EXPAND_HEIGHT;
                this.RibbonPanel.Location = new System.Drawing.Point(0, RIBBON_EXPAND_HEIGHT);
                this.RibbonPanel.Height = this.Height - RIBBON_EXPAND_HEIGHT - FORM_BORDER_HEIGHT;
                this._isRibbonTabExpand = true;
                this._isRibbonTabShow = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonTabContainer_MouseClick(object sender, MouseEventArgs e)
        {
            if (!this._isRibbonTabExpand)
            {
                if (!this._isRibbonTabShow)
                {
                    this.RibbonTabContainer.Height = RIBBON_EXPAND_HEIGHT;
                    this.RibbonTabContainer.BringToFront();
                    this._isRibbonTabShow = true;
                }
                else
                {
                    this.RibbonTabContainer.Height = RIBBON_COLLAPSE_HEIGHT;
                    this._isRibbonTabShow = false;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonTabContainer_Selected(object sender, TabControlEventArgs e)
        {
            this._isRibbonTabShow = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonTabContainer_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (!this._isRibbonTabExpand)
            {
                this.RibbonTabContainer.Height = RIBBON_EXPAND_HEIGHT;
                this.RibbonTabContainer.BringToFront();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideRibbon(object sender, EventArgs e)
        {
            if (!this._isRibbonTabExpand)
            {
                if (this._isRibbonTabShow)
                {
                    this.RibbonTabContainer.Height = RIBBON_COLLAPSE_HEIGHT;
                    this._isRibbonTabShow = false;
                }
            }
        }
        #endregion
    }
}
