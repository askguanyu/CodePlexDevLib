//-----------------------------------------------------------------------
// <copyright file="AssemblyAccessor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Reflection
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Assembly accessor.
    /// </summary>
    public class AssemblyAccessor
    {
        /// <summary>
        /// Field _assembly.
        /// </summary>
        private readonly Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAccessor"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        public AssemblyAccessor(Assembly assembly)
        {
            this._assembly = assembly;
        }

        /// <summary>
        /// Gets Assembly Title.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Title.</returns>
        public static string AssemblyTitle(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

                if (!string.IsNullOrEmpty(titleAttribute.Title))
                {
                    return titleAttribute.Title;
                }
            }

            return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }

        /// <summary>
        /// Gets Assembly Version.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Version.</returns>
        public static string AssemblyVersion(Assembly assembly)
        {
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Gets Assembly Description.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Description.</returns>
        public static string AssemblyDescription(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// Gets Assembly Product.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Product.</returns>
        public static string AssemblyProduct(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyProductAttribute)attributes[0]).Product;
        }

        /// <summary>
        /// Gets Assembly Copyright.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Copyright.</returns>
        public static string AssemblyCopyright(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        /// <summary>
        /// Gets Assembly Company.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly Company.</returns>
        public static string AssemblyCompany(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }

        /// <summary>
        /// Gets Assembly FileVersion.
        /// </summary>
        /// <param name="assembly">The assembly to get.</param>
        /// <returns>Assembly FileVersion.</returns>
        public static string AssemblyFileVersion(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyFileVersionAttribute)attributes[0]).Version;
        }

        /// <summary>
        /// Gets Assembly Title.
        /// </summary>
        /// <returns>Assembly Title.</returns>
        public string AssemblyTitle()
        {
            return AssemblyTitle(this._assembly);
        }

        /// <summary>
        /// Gets Assembly Version.
        /// </summary>
        /// <returns>Assembly Version.</returns>
        public string AssemblyVersion()
        {
            return AssemblyVersion(this._assembly);
        }

        /// <summary>
        /// Gets Assembly Description.
        /// </summary>
        /// <returns>Assembly Description.</returns>
        public string AssemblyDescription()
        {
            return AssemblyDescription(this._assembly);
        }

        /// <summary>
        /// Gets Assembly Product.
        /// </summary>
        /// <returns>Assembly Product.</returns>
        public string AssemblyProduct()
        {
            return AssemblyProduct(this._assembly);
        }

        /// <summary>
        /// Gets Assembly Copyright.
        /// </summary>
        /// <returns>Assembly Copyright.</returns>
        public string AssemblyCopyright()
        {
            return AssemblyCopyright(this._assembly);
        }

        /// <summary>
        /// Gets Assembly Company.
        /// </summary>
        /// <returns>Assembly Company.</returns>
        public string AssemblyCompany()
        {
            return AssemblyCompany(this._assembly);
        }

        /// <summary>
        /// Gets Assembly FileVersion.
        /// </summary>
        /// <returns>Assembly FileVersion.</returns>
        public string AssemblyFileVersion()
        {
            return AssemblyFileVersion(this._assembly);
        }
    }
}
