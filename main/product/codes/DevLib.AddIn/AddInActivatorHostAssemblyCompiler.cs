//-----------------------------------------------------------------------
// <copyright file="AddInActivatorHostAssemblyCompiler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.CSharp;

    /// <summary>
    /// Generates an assembly to run in a separate process in order to host an AddInActivator.
    /// </summary>
    internal static class AddInActivatorHostAssemblyCompiler
    {
        /// <summary>
        ///
        /// </summary>
        private const string OutputAssemblyFileStringFormat = @"DevLib.AddIn.{0}.exe";

        /// <summary>
        ///
        /// </summary>
        private static readonly string[] ReferencedAssemblies = new[] { "System.dll" };

        /// <summary>
        ///
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="addInDomainSetup"></param>
        /// <returns></returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static string CreateRemoteHostAssembly(string friendlyName, AddInDomainSetup addInDomainSetup)
        {
            if (!Directory.Exists(addInDomainSetup.ExeFileDirectory))
            {
                Directory.CreateDirectory(addInDomainSetup.ExeFileDirectory);
            }

            // Dictionary<string, string> providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v2.0" } };

            CompilerResults results = null;

            // using (CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions))
            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                List<string> compilerArgs = new List<string> { AddInPlatformTarget.GetPlatformTargetCompilerArgument(addInDomainSetup.Platform) };

                CompilerParameters compilerParameters = new CompilerParameters
                {
                    // CompilerOptions = string.Join(" ", compilerArgs.ToArray()),
                    GenerateExecutable = true,
                    GenerateInMemory = false,
                    OutputAssembly = Path.Combine(addInDomainSetup.ExeFileDirectory, string.Format(OutputAssemblyFileStringFormat, friendlyName))
                };

                compilerParameters.ReferencedAssemblies.AddRange(ReferencedAssemblies);

                string assemblySource = Properties.Resources.Program
                    .Replace("$[AddInActivatorHostTypeName]", typeof(AddInActivatorHost).AssemblyQualifiedName)
                    .Replace("$[AddInAssemblyName]", typeof(AddInActivatorHost).Assembly.FullName);

                results = provider.CompileAssemblyFromSource(compilerParameters, assemblySource);
            }

            if (results.Errors.HasErrors)
            {
                AddInAssemblyCompilerException addInAssemblyCompilerException = new AddInAssemblyCompilerException("Failed to compile assembly for AddInDomain due to compiler errors.", results.Errors);

                Debug.WriteLine(string.Format(AddInConstants.ExceptionStringFormat, "DevLib.AddIn.AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly", results.ToString(), addInAssemblyCompilerException.ToString(), results.Output.ToString(), string.Empty));

                throw addInAssemblyCompilerException;
            }

            if (results.Errors.HasWarnings)
            {
                AddInAssemblyCompilerException addInAssemblyCompilerException = new AddInAssemblyCompilerException("Succeed to compile assembly for AddInDomain with warnings.", results.Errors);

                Debug.WriteLine(string.Format(AddInConstants.WarningStringFormat, "DevLib.AddIn.AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly", results.ToString(), addInAssemblyCompilerException.ToString(), results.Output.ToString(), string.Empty));
            }

            return results.PathToAssembly;
        }
    }
}
