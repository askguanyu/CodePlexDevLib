//-----------------------------------------------------------------------
// <copyright file="PropertyGridUserControl.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Class PropertyGridUserControl.
    /// </summary>
    public partial class PropertyGridUserControl : UserControl
    {
        /// <summary>
        /// Field _configObjectType.
        /// </summary>
        private Type _configObjectType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridUserControl" /> class.
        /// </summary>
        public PropertyGridUserControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        /// <summary>
        /// Gets or sets the object for which the grid displays properties.
        /// </summary>
        public object ConfigObject
        {
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                object configObject = null;

                try
                {
                    Type configObjectType = this.propertyGrid.SelectedObject.GetType();

                    if (configObjectType.IsGenericType && configObjectType.GetGenericTypeDefinition().IsAssignableFrom(typeof(InnerConfig<>)))
                    {
                        configObject = typeof(InnerConfig<>).MakeGenericType(this._configObjectType).GetProperty("Items").GetValue(this.propertyGrid.SelectedObject, null);
                    }
                    else
                    {
                        configObject = this.propertyGrid.SelectedObject;
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                return configObject;
            }

            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            set
            {
                if (value == null)
                {
                    this.propertyGrid.SelectedObject = null;
                }
                else
                {
                    if (!(value is ICollection))
                    {
                        this.propertyGrid.SelectedObject = value;
                    }
                    else
                    {
                        this._configObjectType = value.GetType();
                        object innerConfig = Activator.CreateInstance(typeof(InnerConfig<>).MakeGenericType(this._configObjectType), value);
                        this.propertyGrid.SelectedObject = innerConfig;
                    }
                }

                this.propertyGrid.PropertySort = PropertySort.NoSort;
                this.propertyGrid.ExpandAllGridItems();
                this.propertyGrid.Refresh();
            }
        }

        /// <summary>
        /// Method OnPropertyValueChanged.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnPropertyValueChanged(object sender, EventArgs e)
        {
            this.RaiseEvent(this.PropertyValueChanged);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Inner Class InnerConfig.
        /// </summary>
        /// <typeparam name="T">Type of configuration object.</typeparam>
        protected class InnerConfig<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InnerConfig{T}" /> class.
            /// </summary>
            /// <param name="configurationObject">Configuration object.</param>
            public InnerConfig(T configurationObject)
            {
                this.Items = configurationObject;
            }

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
}
