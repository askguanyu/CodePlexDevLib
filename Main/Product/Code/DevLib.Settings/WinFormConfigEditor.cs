﻿namespace DevLib.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// Provides a user interface for browsing the properties of an object.
    /// </summary>
    public partial class WinFormConfigEditor : Form
    {
        private IWinFormConfigEditorPlugin _currentConfigEditorPlugin;

        private List<IWinFormConfigEditorPlugin> _configEditorPluginList = new List<IWinFormConfigEditorPlugin>();

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
        private OpenFileDialog _openConfigFileDialog;

        /// <summary>
        /// Field _openPluginFileDialog.
        /// </summary>
        private OpenFileDialog _openPluginFileDialog;

        /// <summary>
        /// Field _saveFileDialog.
        /// </summary>
        private SaveFileDialog _saveConfigFileDialog;

        /// <summary>
        /// Field _isChanged.
        /// </summary>
        private bool _isChanged;

        public delegate TResult OpenDelegate<T, TResult>(T arg);

        public delegate void SaveDelegate<T1, T2>(T1 arg1, T2 arg2);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type of configuration object.</typeparam>
        /// <param name="openDelegate">How to get configuration object from a file.</param>
        /// <param name="saveDelegate">How to save configuration object to a file.</param>
        public void AddPlugin<T>(OpenDelegate<string, T> openDelegate, SaveDelegate<string, T> saveDelegate, string configEditorPluginName = null)
        {
            IWinFormConfigEditorPlugin configEditorPlugin = new InnerPlugin<T>(openDelegate, saveDelegate);
            configEditorPlugin.PluginName = configEditorPluginName;
            this._configEditorPluginList.Add(configEditorPlugin);
            this.toolStripComboBoxConfigEditorPlugin.Items.Add(configEditorPlugin.PluginName);
            this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = this._configEditorPluginList.IndexOf(configEditorPlugin);
        }

        public void AddPlugin(params IWinFormConfigEditorPlugin[] configEditorPluginList)
        {
            if (configEditorPluginList != null && configEditorPluginList.Length > 0)
            {
                foreach (IWinFormConfigEditorPlugin item in configEditorPluginList)
                {
                    if (!ConfigEditorPluginListContainsType(item))
                    {
                        this._configEditorPluginList.Add(item);
                        this.toolStripComboBoxConfigEditorPlugin.Items.Add(item.PluginName ?? item.GetType().Name);
                    }
                }
                this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = 0;
            }
        }

        private bool ConfigEditorPluginListContainsType(IWinFormConfigEditorPlugin configEditorPlugin)
        {
            if (this._configEditorPluginList == null || this._configEditorPluginList.Count < 1)
            {
                return false;
            }

            Type sourceType = configEditorPlugin.GetType();

            foreach (IWinFormConfigEditorPlugin item in this._configEditorPluginList)
            {
                if (sourceType.Equals(item.GetType()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormConfigEditor{T}" /> class.
        /// </summary>
        public WinFormConfigEditor(bool autoLoadPlugin = true)
        {
            this.InitializeComponent();
            this.InitializeFileDialog();
            this.FormTitle = this.Text;

            if (autoLoadPlugin)
            {
                LoadPluginFolder(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            }
        }

        private void LoadPluginFolder(string path)
        {
            var pluginFileList = Directory.GetFiles(path, "*.dll");

            if (pluginFileList != null && pluginFileList.Length > 0)
            {
                foreach (string item in pluginFileList)
                {
                    this.AddPlugin(this.LoadPluginFile(item).ToArray());
                }
            }

            pluginFileList = Directory.GetFiles(path, "*.exe");

            if (pluginFileList != null && pluginFileList.Length > 0)
            {
                foreach (string item in pluginFileList)
                {
                    this.AddPlugin(this.LoadPluginFile(item).ToArray());
                }
            }
        }

        private List<IWinFormConfigEditorPlugin> LoadPluginFile(string assemblyFile)
        {
            List<IWinFormConfigEditorPlugin> result = new List<IWinFormConfigEditorPlugin>();

            if (string.IsNullOrEmpty(assemblyFile))
            {
                return result;
            }

            if (!File.Exists(assemblyFile))
            {
                return result;
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Type[] assemblyTypeList = assembly.GetTypes();

                foreach (Type type in assemblyTypeList)
                {
                    if (type.IsPublic && !type.IsInterface && typeof(IWinFormConfigEditorPlugin).IsAssignableFrom(type))
                    {
                        try
                        {
                            result.Add(assembly.CreateInstance(type.FullName) as IWinFormConfigEditorPlugin);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }

            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormConfigEditor{T}" /> class.
        /// </summary>
        public WinFormConfigEditor(params IWinFormConfigEditorPlugin[] configEditorPluginList)
        {
            this.InitializeComponent();
            this.InitializeFileDialog();
            this.FormTitle = this.Text;

            this.AddPlugin(configEditorPluginList);
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

                if (this._configFile == null)
                {
                    this.Text = this.FormTitle;
                }
                else
                {
                    this.Text = string.Format(FormTitleStringFormat, string.IsNullOrEmpty(this._configFile) ? "Untitled" : this._configFile, this.FormTitle);
                }
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
        private void OpenConfigFile(string fileName)
        {
            try
            {
                this.RefreshPropertyGrid(this._currentConfigEditorPlugin.Open(fileName));
                this.ConfigFile = fileName;
                this.IsChanged = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Could not load configuration from\r\n\"{0}\"\r\n\r\n{1}", fileName, e.ToString()));
            }
        }

        /// <summary>
        /// Save configuration object to a file.
        /// </summary>
        /// <param name="fileName">Configuration file name.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void SaveConfigFile(string fileName)
        {
            object configObject = null;

            try
            {
                if (!(this.propertyGrid.SelectedObject is InnerConfig))
                {
                    configObject = this.propertyGrid.SelectedObject;
                }
                else
                {
                    configObject = (this.propertyGrid.SelectedObject as InnerConfig).Items;
                }

                this._currentConfigEditorPlugin.Save(fileName, configObject);

                this.ConfigFile = fileName;
                this.IsChanged = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Could not save configuration to\r\n\"{0}\"\r\n\r\n{1}", fileName, e.ToString()));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = this.FormTitle;
            this.ConfigFile = null;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = this.SaveConfigDialog();
            base.OnClosing(e);
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
                this.RefreshPropertyGrid(Activator.CreateInstance(this._currentConfigEditorPlugin.ConfigObjectType));
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
                switch (this._openConfigFileDialog.ShowDialog())
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
                        this.OpenConfigFile(this._openConfigFileDialog.FileName);
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
        /// Method RefreshPropertyGrid.
        /// </summary>
        /// <param name="configObject">Instance of T.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void RefreshPropertyGrid(object configObject)
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
            if (this._openConfigFileDialog == null)
            {
                this._openConfigFileDialog = new OpenFileDialog();
            }

            this._openConfigFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this._openConfigFileDialog.Filter = "Configuration Files (*.xml;*.config)|*.xml;*.config|All Files (*.*)|*.*";

            if (this._openPluginFileDialog == null)
            {
                this._openPluginFileDialog = new OpenFileDialog();
            }

            this._openPluginFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this._openPluginFileDialog.Filter = "Plugin Files (*.dll;*.exe)|*.dll;*.exe|All Files (*.*)|*.*";

            if (this._saveConfigFileDialog == null)
            {
                this._saveConfigFileDialog = new SaveFileDialog();
            }

            this._saveConfigFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this._saveConfigFileDialog.Filter = "Configuration Files (*.xml;*.config)|*.xml;*.config|All Files (*.*)|*.*";
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
            switch (this._saveConfigFileDialog.ShowDialog())
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
                    this.SaveConfigFile(this._saveConfigFileDialog.FileName);
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
            public object Items
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Inner Class InnerPlugin.
        /// </summary>
        protected class InnerPlugin<T> : IWinFormConfigEditorPlugin
        {
            private OpenDelegate<string, T> _openDelegate;
            private SaveDelegate<string, T> _saveDelegate;

            private string _pluginName;

            public InnerPlugin(OpenDelegate<string, T> openDelegate, SaveDelegate<string, T> saveDelegate)
            {
                this._openDelegate = openDelegate;
                this._saveDelegate = saveDelegate;

                this.ConfigObjectType = typeof(T);
            }

            public string PluginName
            {
                get
                {
                    return string.IsNullOrEmpty(this._pluginName) ? this.ConfigObjectType.Name : this._pluginName;
                }
                set
                {
                    this._pluginName = string.IsNullOrEmpty(value) ? this.ConfigObjectType.Name : value;
                }
            }

            public Type ConfigObjectType
            {
                get;
                set;
            }

            public object Open(string fileName)
            {
                return this._openDelegate.Invoke(fileName);
            }

            public void Save(string fileName, object configObject)
            {
                this._saveDelegate.Invoke(fileName, (T)configObject);
            }
        }

        private void OnToolStripComboBoxConfigEditorPluginSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != this._configEditorPluginList[this.toolStripComboBoxConfigEditorPlugin.SelectedIndex])
            {
                if (!this.SaveConfigDialog())
                {
                    this.propertyGrid.SelectedObject = null;
                    this.ConfigFile = null;
                    this._isChanged = false;
                    this._currentConfigEditorPlugin = this._configEditorPluginList[this.toolStripComboBoxConfigEditorPlugin.SelectedIndex];
                }
                else
                {
                    this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = this._configEditorPluginList.IndexOf(this._currentConfigEditorPlugin);
                }
            }
        }

        private void OnToolStripButtonOpenPluginClick(object sender, EventArgs e)
        {
            switch (this._openPluginFileDialog.ShowDialog())
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
                    this.AddPlugin(this.LoadPluginFile(this._openPluginFileDialog.FileName).ToArray());
                    break;
                default:
                    break;
            }
        }
    }
}
