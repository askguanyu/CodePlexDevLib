//-----------------------------------------------------------------------
// <copyright file="WinFormConfigEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.WinForms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// Provides a user interface for browsing the properties of an object.
    /// </summary>
    /// <typeparam name="T">Type of configuration object.</typeparam>
    public partial class WinFormConfigEditor<T> : Form where T : new()
    {
        /// <summary>
        /// Field FormTitleStringFormat.
        /// </summary>
        private const string FormTitleStringFormat = "{0} - {1}";

        /// <summary>
        /// Field _configFile.
        /// </summary>
        private string _configFile;

        /// <summary>
        /// Field _openFileDialog.
        /// </summary>
        private OpenFileDialog _openFileDialog;

        /// <summary>
        /// Field _saveFileDialog.
        /// </summary>
        private SaveFileDialog _saveFileDialog;

        /// <summary>
        /// Field _isChanged.
        /// </summary>
        private bool _isChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormConfigEditor{T}" /> class.
        /// </summary>
        /// <param name="openDelegate">How to get configuration object from a file.</param>
        /// <param name="saveDelegate">How to save configuration object to a file.</param>
        public WinFormConfigEditor(Func<string, T> openDelegate, Action<string, T> saveDelegate)
        {
            this.InitializeComponent();
            this.InitializeFileDialog();
            this.FormTitle = this.Text;
            this.ConfigFile = string.Empty;
            this.OpenDelegate = openDelegate;
            this.SaveDelegate = saveDelegate;
            this.RefreshPropertyGrid(new T());
            this.IsChanged = true;
        }

        /// <summary>
        /// Gets or sets this instance form title.
        /// </summary>
        public string FormTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets OpenDelegate.
        /// </summary>
        public Func<string, T> OpenDelegate
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets SaveDelegate.
        /// </summary>
        public Action<string, T> SaveDelegate
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets configuration file.
        /// </summary>
        public string ConfigFile
        {
            get
            {
                return this._configFile;
            }

            private set
            {
                this._configFile = value;
                this.Text = string.Format(FormTitleStringFormat, string.IsNullOrEmpty(this._configFile) ? "Untitled" : this._configFile, this.FormTitle);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether configuration object is changed.
        /// </summary>
        protected bool IsChanged
        {
            get
            {
                return this._isChanged;
            }

            set
            {
                this._isChanged = value;

                if (this._isChanged)
                {
                    if (!this.Text.StartsWith("*"))
                    {
                        this.Text = "*" + this.Text;
                    }
                }
                else
                {
                    this.Text = this.Text.TrimStart('*');
                }
            }
        }

        /// <summary>
        /// Open a file and get configuration object.
        /// </summary>
        /// <param name="fileName">Configuration file name.</param>
        public void OpenConfigFile(string fileName)
        {
            try
            {
                this.RefreshPropertyGrid(this.OpenDelegate(fileName));
                this.ConfigFile = fileName;
                this.IsChanged = false;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Save configuration object to a file.
        /// </summary>
        /// <param name="fileName">Configuration file name.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void SaveConfigFile(string fileName)
        {
            try
            {
                T configObject;

                if (!(this.propertyGrid.SelectedObject is InnerConfig))
                {
                    configObject = (T)this.propertyGrid.SelectedObject;
                }
                else
                {
                    configObject = (this.propertyGrid.SelectedObject as InnerConfig).Items;
                }

                this.SaveDelegate(fileName, configObject);

                this.IsChanged = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Could not save configuration to\r\n\"{0}\"\r\n\r\n{1}", fileName, e.ToString()));
            }
        }

        /// <summary>
        /// Method OnToolStripButtonNewClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonNewClick(object sender, EventArgs e)
        {
            if (!this.SaveConfigDialog())
            {
                this.ConfigFile = string.Empty;
                this.RefreshPropertyGrid(new T());
                this.IsChanged = true;
            }
        }

        /// <summary>
        /// Method OnToolStripButtonOpenClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonOpenClick(object sender, EventArgs e)
        {
            if (!this.SaveConfigDialog())
            {
                switch (this._openFileDialog.ShowDialog())
                {
                    case DialogResult.Abort:
                    case DialogResult.Cancel:
                    case DialogResult.Ignore:
                    case DialogResult.No:
                    case DialogResult.None:
                        break;
                    case DialogResult.OK:
                    case DialogResult.Retry:
                    case DialogResult.Yes:

                        string configFile = this._openFileDialog.FileName;

                        try
                        {
                            this.OpenConfigFile(configFile);
                            this.ConfigFile = configFile;
                            this.IsChanged = false;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("Could not load configuration from\r\n\"{0}\"\r\n\r\n{1}", configFile, ex.ToString()));
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Method OnToolStripButtonSaveClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonSaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.ConfigFile))
            {
                this.SaveAsConfigDialog();
            }
            else
            {
                this.SaveConfigFile(this.ConfigFile);
            }
        }

        /// <summary>
        /// Method OnToolStripButtonSaveAsClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonSaveAsClick(object sender, EventArgs e)
        {
            this.SaveAsConfigDialog();
        }

        /// <summary>
        /// Method OnWinFormConfigEditorFormClosing.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnWinFormConfigEditorFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = this.SaveConfigDialog();
        }

        /// <summary>
        /// Method RefreshPropertyGrid.
        /// </summary>
        /// <param name="configObject">Instance of T.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void RefreshPropertyGrid(T configObject)
        {
            if (!(configObject is ICollection))
            {
                this.propertyGrid.SelectedObject = configObject;
            }
            else
            {
                this.propertyGrid.SelectedObject = new InnerConfig { Items = configObject };
            }

            this.propertyGrid.PropertySort = PropertySort.NoSort;
            this.propertyGrid.ExpandAllGridItems();
            this.IsChanged = false;
        }

        /// <summary>
        /// Method InitializeFileDialog.
        /// </summary>
        private void InitializeFileDialog()
        {
            if (this._openFileDialog == null)
            {
                this._openFileDialog = new OpenFileDialog();
            }

            this._openFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this._openFileDialog.Filter = "Configuration files (*.xml)|*.xml|(*.config)|*.config|All files (*.*)|*.*";

            if (this._saveFileDialog == null)
            {
                this._saveFileDialog = new SaveFileDialog();
            }

            this._saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this._saveFileDialog.Filter = "Configuration files (*.xml)|*.xml|(*.config)|*.config|All files (*.*)|*.*";
        }

        /// <summary>
        /// Method SaveConfigDialog.
        /// </summary>
        /// <returns>true if the event should be canceled; otherwise, false.</returns>
        private bool SaveConfigDialog()
        {
            if (this.IsChanged)
            {
                switch (MessageBox.Show(string.Format("Do you want to save changes to\r\n\"{0}\"", string.IsNullOrEmpty(this.ConfigFile) ? "Untitled" : this.ConfigFile), this.FormTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3))
                {
                    case DialogResult.Yes:

                        if (string.IsNullOrEmpty(this.ConfigFile))
                        {
                            this.SaveAsConfigDialog();
                        }
                        else
                        {
                            this.SaveConfigFile(this.ConfigFile);
                        }

                        return false;

                    case DialogResult.No:
                        return false;
                    case DialogResult.Cancel:
                        return true;
                    default:
                        return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method SaveAsConfigDialog.
        /// </summary>
        private void SaveAsConfigDialog()
        {
            switch (this._saveFileDialog.ShowDialog())
            {
                case DialogResult.Abort:
                case DialogResult.Cancel:
                case DialogResult.Ignore:
                case DialogResult.No:
                case DialogResult.None:
                    break;
                case DialogResult.OK:
                case DialogResult.Retry:
                case DialogResult.Yes:

                    string configFile = this._saveFileDialog.FileName;

                    try
                    {
                        this.SaveConfigFile(configFile);
                        this.ConfigFile = configFile;
                        this.IsChanged = false;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(string.Format("Could not save configuration to\r\n\"{0}\"\r\n\r\n{1}", configFile, e.ToString()));
                    }

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Method OnPropertyGridPropertyValueChanged.
        /// </summary>
        /// <param name="s">Event sender.</param>
        /// <param name="e">Instance of PropertyValueChangedEventArgs.</param>
        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.IsChanged = true;
        }

        /// <summary>
        /// Inner Class InnerConfig.
        /// </summary>
        protected class InnerConfig
        {
            /// <summary>
            /// Gets or sets Items.
            /// </summary>
            [Editor(typeof(PropertyValueChangedCollectionEditor), typeof(UITypeEditor))]
            public T Items
            {
                get;
                set;
            }
        }
    }

    /// <summary>
    /// Provides a user interface that can edit most types of collections at design time. Collection changing can raise PropertyValueChanged.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class PropertyValueChangedCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueChangedCollectionEditor" /> class using the specified collection type.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public PropertyValueChangedCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Edits the value of the specified object using the specified service provider and context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services can be obtained.</param>
        /// <param name="value">The object to edit the value of.</param>
        /// <returns>The new value of the object.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object source = base.EditValue(context, provider, value);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    /// <summary>
    /// Provides a type converter to convert expandable objects to and from various other representations.
    /// </summary>
    /// <typeparam name="T">Type of object to convert.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class ExpandableObjectConverter<T> : ExpandableObjectConverter where T : new()
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null || string.IsNullOrEmpty(value as string))
            {
                return null;
            }
            else
            {
                return new T();
            }
        }
    }
}
