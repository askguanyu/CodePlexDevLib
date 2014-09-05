//-----------------------------------------------------------------------
// <copyright file="IWinFormConfigEditorPlugin.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;

    /// <summary>
    /// Interface for WinFormConfigEditor to process configuration object plugin.
    /// </summary>
    public interface IWinFormConfigEditorPlugin
    {
        /// <summary>
        /// Gets or sets plugin name.
        /// </summary>
        string PluginName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets configuration object type.
        /// </summary>
        Type ConfigObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// How to get configuration object from a file.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Configuration object.</returns>
        object Open(string filename);

        /// <summary>
        /// How to save configuration object to a file.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="configObject">Configuration object.</param>
        void Save(string filename, object configObject);
    }
}
