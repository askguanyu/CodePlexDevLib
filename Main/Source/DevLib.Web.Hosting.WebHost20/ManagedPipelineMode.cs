//-----------------------------------------------------------------------
// <copyright file="ManagedPipelineMode.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    /// <summary>
    /// The request-processing pipeline mode of managed applications in the application pool.
    /// </summary>
    public enum ManagedPipelineMode
    {
        /// <summary>
        /// Integrated mode allows IIS to process requests in the application pool by using the IIS 7 and later integrated pipeline. This allows ASP.NET modules to participate in IIS request processing regardless of the type of resource requested. Using integrated mode makes available features of the ASP.NET 2.0 request pipeline available to requests for static content, as well as ASP, PHP and other content types. By default, IIS 7 and later application pools run in this mode.
        /// </summary>
        Integrated,

        /// <summary>
        /// Classic mode uses the IIS 6.0 processing pipeline for hosting ASP.NET applications. In this mode, requests are processed initially through IIS 7 and later modules, and ASP.NET requests are further processed by the aspnet_isapi.dll. The ASP.NET processing pipeline is separate from the IIS 7 and later processing pipeline, and the ASP.NET request processing pipeline features are not available to other resource types. This also means that an ASP.NET request must pass through authentication and authorization modules in both process models. While this is not as efficient as Integrated mode, it does allow you to run applications developed using ASP.NET version 1.1 on an IIS 7 and later server without modifying the application to run in Integrated mode.
        /// </summary>
        Classic
    }
}
