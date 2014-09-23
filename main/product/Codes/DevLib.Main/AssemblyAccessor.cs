//-----------------------------------------------------------------------
// <copyright file="AssemblyAccessor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Main
{
    using System.Reflection;

    /// <summary>
    /// Static Class AssemblyAccessor.
    /// </summary>
    public static class AssemblyAccessor
    {
        /// <summary>
        /// Gets Assembly Title.
        /// </summary>
        /// <returns>Assembly Title.</returns>
        public static string AssemblyTitle()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                {
                    return titleAttribute.Title;
                }
            }

            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }

        /// <summary>
        /// Gets Assembly Version.
        /// </summary>
        /// <returns>Assembly Version.</returns>
        public static string AssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Gets Assembly Description.
        /// </summary>
        /// <returns>Assembly Description.</returns>
        public static string AssemblyDescription()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// Gets Assembly Product.
        /// </summary>
        /// <returns>Assembly Product.</returns>
        public static string AssemblyProduct()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyProductAttribute)attributes[0]).Product;
        }

        /// <summary>
        /// Gets Assembly Copyright.
        /// </summary>
        /// <returns>Assembly Copyright.</returns>
        public static string AssemblyCopyright()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        /// <summary>
        /// Gets Assembly Company.
        /// </summary>
        /// <returns>Assembly Company.</returns>
        public static string AssemblyCompany()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }

        /// <summary>
        /// Gets Assembly FileVersion.
        /// </summary>
        /// <returns>Assembly FileVersion.</returns>
        public static string AssemblyFileVersion()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyFileVersionAttribute)attributes[0]).Version;
        }
    }
}
