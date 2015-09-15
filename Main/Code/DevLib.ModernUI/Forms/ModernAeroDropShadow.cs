//-----------------------------------------------------------------------
// <copyright file="ModernAeroDropShadow.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System.Windows.Forms;

    /// <summary>
    /// ModernAeroDropShadow class.
    /// </summary>
    internal class ModernAeroDropShadow : ModernShadowBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernAeroDropShadow"/> class.
        /// </summary>
        /// <param name="targetForm">The target form.</param>
        public ModernAeroDropShadow(Form targetForm)
            : base(targetForm, 0, ModernShadowBase.WS_EX_TRANSPARENT | ModernShadowBase.WS_EX_NOACTIVATE)
        {
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        }

        /// <summary>
        /// SetBoundsCore method.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="width">The bounds width.</param>
        /// <param name="height">The bounds height.</param>
        /// <param name="specified">A value from the BoundsSpecified enumeration.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (specified == BoundsSpecified.Size)
            {
                return;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        /// <summary>
        /// Paints the shadow.
        /// </summary>
        protected override void PaintShadow()
        {
            this.Visible = true;
        }

        /// <summary>
        /// Clears the shadow.
        /// </summary>
        protected override void ClearShadow()
        {
        }
    }
}
