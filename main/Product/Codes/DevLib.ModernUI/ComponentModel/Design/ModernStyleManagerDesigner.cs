//-----------------------------------------------------------------------
// <copyright file="ModernStyleManagerDesigner.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// Extends the design mode behavior of ModernStyleManager.
    /// </summary>
    internal class ModernStyleManagerDesigner : ComponentDesigner
    {
        /// <summary>
        /// Field _designerVerbCollection.
        /// </summary>
        private DesignerVerbCollection _designerVerbCollection;

        /// <summary>
        /// Field _designerHost.
        /// </summary>
        private IDesignerHost _designerHost;

        /// <summary>
        /// Field _componentChangeService.
        /// </summary>
        private IComponentChangeService _componentChangeService;

        /// <summary>
        /// Gets the design-time verbs supported by the component that is associated with the designer.
        /// </summary>
        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this._designerVerbCollection != null)
                {
                    return this._designerVerbCollection;
                }

                this._designerVerbCollection = new DesignerVerbCollection();
                this._designerVerbCollection.Add(new DesignerVerb("Reset Styles to Default", this.OnResetStyles));

                return this._designerVerbCollection;
            }
        }

        /// <summary>
        /// Gets instance of IDesignerHost.
        /// </summary>
        public IDesignerHost DesignerHost
        {
            get
            {
                if (this._designerHost != null)
                {
                    return this._designerHost;
                }

                this._designerHost = (IDesignerHost)this.GetService(typeof(IDesignerHost));

                return this._designerHost;
            }
        }

        /// <summary>
        /// Gets instance of IComponentChangeService.
        /// </summary>
        public IComponentChangeService ComponentChangeService
        {
            get
            {
                if (this._componentChangeService != null)
                {
                    return this._componentChangeService;
                }

                this._componentChangeService = (IComponentChangeService)this.GetService(typeof(IComponentChangeService));

                return this._componentChangeService;
            }
        }

        /// <summary>
        /// OnResetStyles method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        private void OnResetStyles(object sender, EventArgs e)
        {
            ModernStyleManager styleManager = Component as ModernStyleManager;

            if (styleManager != null)
            {
                if (styleManager.Owner == null)
                {
                    MessageBox.Show("StyleManager needs the Owner property assigned to before it can reset styles.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            this.ResetStyles(styleManager, styleManager.Owner as Control);
        }

        /// <summary>
        /// ResetStyles method.
        /// </summary>
        /// <param name="styleManager">ModernStyleManager instance.</param>
        /// <param name="control">Control to reset.</param>
        private void ResetStyles(ModernStyleManager styleManager, Control control)
        {
            IModernForm container = control as IModernForm;

            if (container != null && !object.ReferenceEquals(styleManager, container.StyleManager))
            {
                return;
            }

            if (control is IModernControl)
            {
                this.ResetProperty(control, "Style", ModernColorStyle.Default);
                this.ResetProperty(control, "Theme", ModernThemeStyle.Default);
            }
            else if (control is IModernComponent)
            {
                this.ResetProperty(control, "Style", ModernColorStyle.Default);
                this.ResetProperty(control, "Theme", ModernThemeStyle.Default);
            }

            if (control.ContextMenuStrip != null)
            {
                this.ResetStyles(styleManager, control.ContextMenuStrip);
            }

            TabControl tabControl = control as TabControl;

            if (tabControl != null)
            {
                foreach (TabPage item in tabControl.TabPages)
                {
                    this.ResetStyles(styleManager, item);
                }
            }

            if (control.Controls != null)
            {
                foreach (Control item in control.Controls)
                {
                    this.ResetStyles(styleManager, item);
                }
            }
        }

        /// <summary>
        /// ResetProperty method.
        /// </summary>
        /// <param name="control">Control to reset.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="newValue">New value.</param>
        private void ResetProperty(Control control, string propertyName, object newValue)
        {
            var typeDescriptor = TypeDescriptor.GetProperties(control)[propertyName];

            if (typeDescriptor == null)
            {
                return;
            }

            object oldValue = typeDescriptor.GetValue(control);

            if (newValue.Equals(oldValue))
            {
                return;
            }

            this.ComponentChangeService.OnComponentChanging(control, typeDescriptor);
            typeDescriptor.SetValue(control, newValue);
            this.ComponentChangeService.OnComponentChanged(control, typeDescriptor, oldValue, newValue);
        }
    }
}
