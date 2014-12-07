//-----------------------------------------------------------------------
// <copyright file="ModernStyleManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel.Design;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// ModernStyleManager class.
    /// </summary>
    [Designer(typeof(ModernStyleManagerDesigner))]
    public sealed class ModernStyleManager : Component, ICloneable, ISupportInitialize
    {
        /// <summary>
        /// Field _parentContainer.
        /// </summary>
        private readonly IContainer _parentContainer;

        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernConstants.DefaultColorStyle;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernConstants.DefaultThemeStyle;

        /// <summary>
        /// Field _owner.
        /// </summary>
        private ContainerControl _owner;

        /// <summary>
        /// Field _isInitializing.
        /// </summary>
        private bool _isInitializing;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleManager" /> class.
        /// </summary>
        public ModernStyleManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleManager" /> class.
        /// </summary>
        /// <param name="parent">Parent container.</param>
        public ModernStyleManager(IContainer parent)
        {
            if (parent != null)
            {
                this._parentContainer = parent;
                this._parentContainer.Add(this);
            }
        }

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [DefaultValue(ModernConstants.DefaultColorStyle)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernColorStyle ColorStyle
        {
            get
            {
                return this._modernColorStyle;
            }

            set
            {
                this._modernColorStyle = value == ModernColorStyle.Default ? ModernConstants.DefaultColorStyle : value;

                if (!this._isInitializing)
                {
                    this.Update();
                }
            }
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        [DefaultValue(ModernConstants.DefaultThemeStyle)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernThemeStyle ThemeStyle
        {
            get
            {
                return this._modernThemeStyle;
            }

            set
            {
                this._modernThemeStyle = value == ModernThemeStyle.Default ? ModernConstants.DefaultThemeStyle : value;

                if (!this._isInitializing)
                {
                    this.Update();
                }
            }
        }

        /// <summary>
        /// Gets or sets owner.
        /// </summary>
        public ContainerControl Owner
        {
            get
            {
                return this._owner;
            }

            set
            {
                if (this._owner != null)
                {
                    this._owner.ControlAdded -= this.ControlAdded;
                }

                this._owner = value;

                if (value != null)
                {
                    this._owner.ControlAdded += this.ControlAdded;

                    if (!this._isInitializing)
                    {
                        this.UpdateControl(value);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            ModernStyleManager result = new ModernStyleManager();
            result._modernThemeStyle = this.ThemeStyle;
            result._modernColorStyle = this.ColorStyle;
            return result;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <param name="owner">Owner to set.</param>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone(ContainerControl owner)
        {
            ModernStyleManager result = this.Clone() as ModernStyleManager;

            if (owner is IModernForm)
            {
                result.Owner = owner;
            }

            return result;
        }

        /// <summary>
        /// Update controls.
        /// </summary>
        public void Update()
        {
            if (this._owner != null)
            {
                this.UpdateControl(this._owner);
            }

            if (this._parentContainer == null || this._parentContainer.Components == null)
            {
                return;
            }

            foreach (object obj in this._parentContainer.Components)
            {
                if (obj is IModernComponent)
                {
                    this.ApplyModernStyle((IModernComponent)obj);
                }

                if (obj.GetType() == typeof(ModernContextMenuStrip))
                {
                    this.ApplyModernStyle((ModernContextMenuStrip)obj);
                }
            }
        }

        /// <summary>
        /// Signals the object that initialization is starting.
        /// </summary>
        void ISupportInitialize.BeginInit()
        {
            this._isInitializing = true;
        }

        /// <summary>
        /// Signals the object that initialization is complete.
        /// </summary>
        void ISupportInitialize.EndInit()
        {
            this._isInitializing = false;
            this.Update();
        }

        /// <summary>
        /// ControlAdded method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">ControlEventArgs instance.</param>
        private void ControlAdded(object sender, ControlEventArgs e)
        {
            if (!this._isInitializing)
            {
                this.UpdateControl(e.Control);
            }
        }

        /// <summary>
        /// Update control.
        /// </summary>
        /// <param name="control">Control to update.</param>
        private void UpdateControl(Control control)
        {
            if (control == null)
            {
                return;
            }

            IModernControl modernControl = control as IModernControl;

            if (modernControl != null)
            {
                this.ApplyModernStyle(modernControl);
            }

            IModernComponent modernComponent = control as IModernComponent;

            if (modernComponent != null)
            {
                this.ApplyModernStyle(modernComponent);
            }

            TabControl tabControl = control as TabControl;

            if (tabControl != null)
            {
                foreach (TabPage item in ((TabControl)control).TabPages)
                {
                    this.UpdateControl(item);
                }
            }

            if (control.Controls != null)
            {
                foreach (Control item in control.Controls)
                {
                    this.UpdateControl(item);
                }
            }

            if (control.ContextMenuStrip != null)
            {
                this.UpdateControl(control.ContextMenuStrip);
            }

            control.Refresh();
        }

        /// <summary>
        /// Apply modern style.
        /// </summary>
        /// <param name="control">IModernControl to apply.</param>
        private void ApplyModernStyle(IModernControl control)
        {
            control.StyleManager = this;
        }

        /// <summary>
        /// Apply modern style.
        /// </summary>
        /// <param name="component">IModernComponent to apply.</param>
        private void ApplyModernStyle(IModernComponent component)
        {
            component.StyleManager = this;
        }
    }
}