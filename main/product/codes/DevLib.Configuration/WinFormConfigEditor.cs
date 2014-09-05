//-----------------------------------------------------------------------
// <copyright file="WinFormConfigEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// Provides a user interface for browsing the properties of an object.
    /// </summary>
    public partial class WinFormConfigEditor : Form
    {
        /// <summary>
        /// Field FormTitleStringFormat.
        /// </summary>
        private const string FormTitleStringFormat = "{0} - {1}";

        /// <summary>
        /// Field _currentConfigEditorPlugin.
        /// </summary>
        private IWinFormConfigEditorPlugin _currentConfigEditorPlugin;

        /// <summary>
        /// Field _configEditorPluginList.
        /// </summary>
        private List<IWinFormConfigEditorPlugin> _configEditorPluginList = new List<IWinFormConfigEditorPlugin>();

        /// <summary>
        /// Field _configFile.
        /// </summary>
        private string _configFile;

        /// <summary>
        /// Field _openConfigFileDialog.
        /// </summary>
        private OpenFileDialog _openConfigFileDialog;

        /// <summary>
        /// Field _openPluginFileDialog.
        /// </summary>
        private OpenFileDialog _openPluginFileDialog;

        /// <summary>
        /// Field _saveConfigFileDialog.
        /// </summary>
        private SaveFileDialog _saveConfigFileDialog;

        /// <summary>
        /// Field _isChanged.
        /// </summary>
        private bool _isChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormConfigEditor" /> class.
        /// </summary>
        /// <param name="autoLoadPlugin">Whether automatically load all possible plugins under current folder or not.</param>
        public WinFormConfigEditor(bool autoLoadPlugin = true)
        {
            this.InitializeComponent();
            this.Initialize();

            if (autoLoadPlugin)
            {
                this.LoadPluginFolder(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

                if (this.toolStripComboBoxConfigEditorPlugin.Items.Count > 0)
                {
                    this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormConfigEditor" /> class.
        /// </summary>
        /// <param name="configEditorPluginList">IWinFormConfigEditorPlugin list.</param>
        public WinFormConfigEditor(params IWinFormConfigEditorPlugin[] configEditorPluginList)
        {
            this.InitializeComponent();
            this.Initialize();

            this.AddPlugin(configEditorPluginList);

            if (this.toolStripComboBoxConfigEditorPlugin.Items.Count > 0)
            {
                this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Delegate of open configuration file.
        /// </summary>
        /// <typeparam name="T">Type of configuration file.</typeparam>
        /// <typeparam name="TResult">Type of configuration object.</typeparam>
        /// <param name="arg">Configuration file.</param>
        /// <returns>Configuration object.</returns>
        public delegate TResult OpenDelegate<T, TResult>(T arg);

        /// <summary>
        /// Delegate of save configuration file.
        /// </summary>
        /// <typeparam name="T1">Type of configuration file.</typeparam>
        /// <typeparam name="T2">Type of configuration object.</typeparam>
        /// <param name="arg1">Configuration file.</param>
        /// <param name="arg2">Configuration object.</param>
        public delegate void SaveDelegate<T1, T2>(T1 arg1, T2 arg2);

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
        /// Add configuration editor plugin.
        /// </summary>
        /// <typeparam name="T">Type of configuration object.</typeparam>
        /// <param name="openDelegate">How to get configuration object from a file.</param>
        /// <param name="saveDelegate">How to save configuration object to a file.</param>
        /// <param name="configEditorPluginName">IWinFormConfigEditorPlugin PluginName.</param>
        public void AddPlugin<T>(OpenDelegate<string, T> openDelegate, SaveDelegate<string, T> saveDelegate, string configEditorPluginName = null)
        {
            IWinFormConfigEditorPlugin configEditorPlugin = new InnerPlugin<T>(openDelegate, saveDelegate);
            configEditorPlugin.PluginName = configEditorPluginName;
            this._configEditorPluginList.Add(configEditorPlugin);
            this.toolStripComboBoxConfigEditorPlugin.Items.Add(configEditorPlugin.PluginName);
            this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = this._configEditorPluginList.IndexOf(configEditorPlugin);
        }

        /// <summary>
        /// Add configuration editor plugin.
        /// </summary>
        /// <param name="configEditorPluginList">ConfigEditorPlugin list.</param>
        public void AddPlugin(params IWinFormConfigEditorPlugin[] configEditorPluginList)
        {
            if (configEditorPluginList != null && configEditorPluginList.Length > 0)
            {
                foreach (IWinFormConfigEditorPlugin item in configEditorPluginList)
                {
                    if (!this.ConfigEditorPluginListContainsType(item))
                    {
                        this._configEditorPluginList.Add(item);
                        this.toolStripComboBoxConfigEditorPlugin.Items.Add(!string.IsNullOrEmpty(item.PluginName) ? item.PluginName : item.GetType().Name);
                    }
                }
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
        /// Method ConfigEditorPluginListContainsType.
        /// </summary>
        /// <param name="configEditorPlugin">IWinFormConfigEditorPlugin instance.</param>
        /// <returns>true if <paramref name="configEditorPlugin"/> already exists; otherwise, false.</returns>
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
        /// Method LoadPluginFolder.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        private void LoadPluginFolder(string path)
        {
            string[] pluginFileList = Directory.GetFiles(path, "*.dll");

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

        /// <summary>
        /// Method LoadPluginFile.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <returns>IWinFormConfigEditorPlugin list.</returns>
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
                            InternalLogger.Log(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return result;
        }

        /// <summary>
        /// Open a file and get configuration object.
        /// </summary>
        /// <param name="filename">Configuration file name.</param>
        private void OpenConfigFile(string filename)
        {
            if (this._currentConfigEditorPlugin != null)
            {
                try
                {
                    this.RefreshPropertyGrid(this._currentConfigEditorPlugin.Open(filename));
                    this.ConfigFile = filename;
                    this.IsChanged = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Could not load configuration from\r\n\"{0}\"\r\n\r\n{1}", filename, e.ToString()));
                }
            }
        }

        /// <summary>
        /// Save configuration object to a file.
        /// </summary>
        /// <param name="filename">Configuration file name.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void SaveConfigFile(string filename)
        {
            if (this._currentConfigEditorPlugin != null)
            {
                try
                {
                    object configObject = this.propertyGridUserControl.ConfigObject;
                    this._currentConfigEditorPlugin.Save(filename, configObject);
                    this.ConfigFile = filename;
                    this.IsChanged = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Could not save configuration to\r\n\"{0}\"\r\n\r\n{1}", filename, e.ToString()));
                }
            }
        }

        /// <summary>
        /// Method OnToolStripButtonNewClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonNewClick(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != null)
            {
                if (!this.SaveConfigDialog())
                {
                    this.ConfigFile = string.Empty;
                    this.RefreshPropertyGrid(Activator.CreateInstance(this._currentConfigEditorPlugin.ConfigObjectType));
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// Method OnToolStripButtonOpenClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonOpenClick(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != null)
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
        }

        /// <summary>
        /// Method OnToolStripButtonSaveClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonSaveClick(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != null)
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
        }

        /// <summary>
        /// Method OnToolStripButtonSaveAsClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripButtonSaveAsClick(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != null)
            {
                this.SaveAsConfigDialog();
            }
        }

        /// <summary>
        /// Method OnToolStripButtonOpenPluginClick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
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

            if (this.toolStripComboBoxConfigEditorPlugin.Items.Count > 0)
            {
                this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Method OnToolStripComboBoxConfigEditorPluginSelectedIndexChanged.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnToolStripComboBoxConfigEditorPluginSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._currentConfigEditorPlugin != this._configEditorPluginList[this.toolStripComboBoxConfigEditorPlugin.SelectedIndex])
            {
                if (!this.SaveConfigDialog())
                {
                    this.RefreshPropertyGrid(null);
                    this.ConfigFile = null;
                    this._currentConfigEditorPlugin = this._configEditorPluginList[this.toolStripComboBoxConfigEditorPlugin.SelectedIndex];
                }
                else
                {
                    this.toolStripComboBoxConfigEditorPlugin.SelectedIndex = this._configEditorPluginList.IndexOf(this._currentConfigEditorPlugin);
                }
            }
        }

        /// <summary>
        /// Method OnToolStripSizeChanged;
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnWinFormConfigEditorSizeChanged(object sender, EventArgs e)
        {
            this.toolStripComboBoxConfigEditorPlugin.Size = new System.Drawing.Size(this.Width - 321, 25);
        }

        /// <summary>
        /// Method OnPropertyGridUserControlPropertyValueChanged.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnPropertyGridUserControlPropertyValueChanged(object sender, EventArgs e)
        {
            this.IsChanged = true;
        }

        /// <summary>
        /// Method RefreshPropertyGrid.
        /// </summary>
        /// <param name="configObject">Instance of T.</param>
        private void RefreshPropertyGrid(object configObject)
        {
            this.propertyGridUserControl.ConfigObject = configObject;

            this.IsChanged = false;
        }

        /// <summary>
        /// Method Initialize.
        /// </summary>
        private void Initialize()
        {
            if (this._openConfigFileDialog == null)
            {
                this._openConfigFileDialog = new OpenFileDialog();
                this._openConfigFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }

            this._openConfigFileDialog.Reset();
            this._openConfigFileDialog.Filter = "Xml Files (*.xml)|*.xml|Configuration Files (*.config)|*.config|All Files (*.*)|*.*";

            if (this._openPluginFileDialog == null)
            {
                this._openPluginFileDialog = new OpenFileDialog();
                this._openPluginFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }

            this._openPluginFileDialog.Reset();
            this._openPluginFileDialog.Filter = "Plugin Files (*.dll;*.exe)|*.dll;*.exe|All Files (*.*)|*.*";

            if (this._saveConfigFileDialog == null)
            {
                this._saveConfigFileDialog = new SaveFileDialog();
                this._saveConfigFileDialog.InitialDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }

            this._saveConfigFileDialog.Reset();
            this._saveConfigFileDialog.Filter = "Xml Files (*.xml)|*.xml|Configuration Files (*.config)|*.config|All Files (*.*)|*.*";

            this.FormTitle = this.Text;
            this.toolStripComboBoxConfigEditorPlugin.Size = new System.Drawing.Size(this.Width - 321, 25);
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
        /// Inner Class InnerPlugin.
        /// </summary>
        /// <typeparam name="T">Type of configuration object.</typeparam>
        protected class InnerPlugin<T> : IWinFormConfigEditorPlugin
        {
            /// <summary>
            /// Field _openDelegate.
            /// </summary>
            private OpenDelegate<string, T> _openDelegate;

            /// <summary>
            /// Field _openDelegate.
            /// </summary>
            private SaveDelegate<string, T> _saveDelegate;

            /// <summary>
            /// Field _pluginName.
            /// </summary>
            private string _pluginName;

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerPlugin{T}" /> class.
            /// </summary>
            /// <param name="openDelegate">Open method delegate.</param>
            /// <param name="saveDelegate">Save method delegate.</param>
            public InnerPlugin(OpenDelegate<string, T> openDelegate, SaveDelegate<string, T> saveDelegate)
            {
                this._openDelegate = openDelegate;
                this._saveDelegate = saveDelegate;

                this.ConfigObjectType = typeof(T);
            }

            /// <summary>
            /// Gets or sets PluginName.
            /// </summary>
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

            /// <summary>
            /// Gets or sets ConfigObjectType.
            /// </summary>
            public Type ConfigObjectType
            {
                get;
                set;
            }

            /// <summary>
            /// Method Open.
            /// </summary>
            /// <param name="filename">Configuration file.</param>
            /// <returns>Configuration object.</returns>
            public object Open(string filename)
            {
                return this._openDelegate.Invoke(filename);
            }

            /// <summary>
            /// Method Save.
            /// </summary>
            /// <param name="filename">Configuration file.</param>
            /// <param name="configObject">Configuration object.</param>
            public void Save(string filename, object configObject)
            {
                this._saveDelegate.Invoke(filename, (T)configObject);
            }
        }
    }
}
