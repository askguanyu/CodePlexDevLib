//-----------------------------------------------------------------------
// <copyright file="ModernTabControlDesigner.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using DevLib.ModernUI.Forms;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernTabControl Designer.
    /// </summary>
    internal class ModernTabControlDesigner : ParentControlDesigner
    {
        /// <summary>
        /// Field _designerVerbCollection.
        /// </summary>
        private readonly DesignerVerbCollection _designerVerbCollection = new DesignerVerbCollection();

        /// <summary>
        /// Field _designerHost.
        /// </summary>
        private IDesignerHost _designerHost;

        /// <summary>
        /// Field _selectionService.
        /// </summary>
        private ISelectionService _selectionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTabControlDesigner" /> class.
        /// </summary>
        public ModernTabControlDesigner()
        {
            var verb1 = new DesignerVerb("Add Tab", this.OnAddPage);
            var verb2 = new DesignerVerb("Remove Tab", this.OnRemovePage);
            this._designerVerbCollection.AddRange(new[] { verb1, verb2 });
        }

        /// <summary>
        /// Gets the selection rules that indicate the movement capabilities of a component.
        /// </summary>
        public override SelectionRules SelectionRules
        {
            get
            {
                return this.Control.Dock == DockStyle.Fill ? SelectionRules.Visible : base.SelectionRules;
            }
        }

        /// <summary>
        /// Gets the design-time verbs supported by the component that is associated with the designer.
        /// </summary>
        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this._designerVerbCollection.Count == 2)
                {
                    var control = (ModernTabControl)this.Control;
                    this._designerVerbCollection[1].Enabled = control.TabCount != 0;
                }

                return this._designerVerbCollection;
            }
        }

        /// <summary>
        /// Gets the designer host.
        /// </summary>
        public IDesignerHost DesignerHost
        {
            get
            {
                return this._designerHost ?? (this._designerHost = (IDesignerHost)GetService(typeof(IDesignerHost)));
            }
        }

        /// <summary>
        /// Gets the selection service.
        /// </summary>
        public ISelectionService SelectionService
        {
            get
            {
                return this._selectionService ?? (this._selectionService = (ISelectionService)GetService(typeof(ISelectionService)));
            }
        }

        /// <summary>
        /// Processes Windows messages and optionally routes them to the control.
        /// </summary>
        /// <param name="m">The <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case (int)WinApi.Messages.WM_NCHITTEST:

                    if (m.Result.ToInt32() == (int)WinApi.HitTest.HTTRANSPARENT)
                    {
                        m.Result = (IntPtr)WinApi.HitTest.HTCLIENT;
                    }

                    break;
            }
        }

        /// <summary>
        /// Indicates whether a mouse click at the specified point should be handled by the control.
        /// </summary>
        /// <param name="point">A <see cref="T:System.Drawing.Point" /> indicating the position at which the mouse was clicked, in screen coordinates.</param>
        /// <returns>true if a click at the specified point is to be handled by the control; otherwise, false.</returns>
        protected override bool GetHitTest(Point point)
        {
            if (this.SelectionService.PrimarySelection == this.Control)
            {
                var hti = new WinApi.TCHITTESTINFO
                {
                    pt = this.Control.PointToClient(point),
                    flags = 0
                };

                var m = new Message
                {
                    HWnd = this.Control.Handle,
                    Msg = WinApi.TCM_HITTEST
                };

                var lparam = Marshal.AllocHGlobal(Marshal.SizeOf(hti));
                Marshal.StructureToPtr(hti, lparam, false);
                m.LParam = lparam;

                base.WndProc(ref m);
                Marshal.FreeHGlobal(lparam);

                if (m.Result.ToInt32() != -1)
                {
                    return hti.flags != (int)WinApi.TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }

        /// <summary>
        /// Adjusts the set of properties the component will expose through a <see cref="T:System.ComponentModel.TypeDescriptor" />.
        /// </summary>
        /// <param name="properties">An <see cref="T:System.Collections.IDictionary" /> that contains the properties for the class of the component.</param>
        protected override void PreFilterProperties(IDictionary properties)
        {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");
            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            properties.Remove("Image");
            properties.Remove("ImageAlign");
            properties.Remove("ImageIndex");
            properties.Remove("ImageKey");
            properties.Remove("ImageList");
            properties.Remove("TextImageRelation");

            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("UseVisualStyleBackColor");

            properties.Remove("Font");
            properties.Remove("RightToLeft");

            base.PreFilterProperties(properties);
        }

        /// <summary>
        /// Called when AddPage.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnAddPage(object sender, EventArgs e)
        {
            var parentControl = (ModernTabControl)this.Control;
            var oldTabs = parentControl.Controls;

            this.RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            var p = (ModernTabPage)this.DesignerHost.CreateComponent(typeof(ModernTabPage));
            p.Text = p.Name;
            parentControl.TabPages.Add(p);

            this.RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"], oldTabs, parentControl.TabPages);
            parentControl.SelectedTab = p;

            this.SetVerbs();
        }

        /// <summary>
        /// Called when RemovePage.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnRemovePage(object sender, EventArgs e)
        {
            var parentControl = (ModernTabControl)Control;
            var oldTabs = parentControl.Controls;

            if (parentControl.SelectedIndex < 0)
            {
                return;
            }

            this.RaiseComponentChanging(TypeDescriptor.GetProperties(parentControl)["TabPages"]);

            this.DesignerHost.DestroyComponent(parentControl.TabPages[parentControl.SelectedIndex]);

            this.RaiseComponentChanged(TypeDescriptor.GetProperties(parentControl)["TabPages"], oldTabs, parentControl.TabPages);

            this.SelectionService.SetSelectedComponents(new IComponent[] { parentControl }, SelectionTypes.Auto);

            this.SetVerbs();
        }

        /// <summary>
        /// Sets the verbs.
        /// </summary>
        private void SetVerbs()
        {
            var parentControl = (ModernTabControl)Control;

            switch (parentControl.TabPages.Count)
            {
                case 0:
                    this.Verbs[1].Enabled = false;
                    break;

                default:
                    this.Verbs[1].Enabled = true;
                    break;
            }
        }
    }
}
