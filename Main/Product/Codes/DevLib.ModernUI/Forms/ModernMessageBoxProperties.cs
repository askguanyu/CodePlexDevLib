//-----------------------------------------------------------------------
// <copyright file="ModernMessageBoxProperties.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// Message box overlay display properties.
    /// </summary>
    public class ModernMessageBoxProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernMessageBoxProperties" /> class.
        /// </summary>
        /// <param name="owner">ModernMessageBoxControl instance.</param>
        public ModernMessageBoxProperties(ModernMessageBoxForm owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets the property owner.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ModernMessageBoxForm Owner
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the message box overlay message contents.
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message box title.
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message box buttons in the message box overlay.
        /// </summary>
        public MessageBoxButtons Buttons
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message box overlay icon.
        /// </summary>
        public MessageBoxIcon Icon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message box default button.
        /// </summary>
        public MessageBoxDefaultButton DefaultButton
        {
            get;
            set;
        }
    }
}
