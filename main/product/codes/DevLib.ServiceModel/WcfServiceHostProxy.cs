//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    /// <summary>
    /// Class WcfServiceHostProxy.
    /// </summary>
    [Serializable]
    internal class WcfServiceHostProxy : ServiceHost
    {
        /// <summary>
        /// Field ConfigSyncRoot.
        /// </summary>
        private static readonly object ConfigSyncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostProxy" /> class.
        /// </summary>
        /// <param name="serviceType">The type of hosted service.</param>
        /// <param name="baseAddresses">An array of type System.Uri that contains the base addresses for the hosted service.</param>
        public WcfServiceHostProxy(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        /// <summary>
        /// Gets or sets configuration file.
        /// </summary>
        public static string ConfigFile
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the service description information from the configuration file and applies it to the runtime being constructed.
        /// </summary>
        protected override void ApplyConfiguration()
        {
            lock (ConfigSyncRoot)
            {
                if (File.Exists(ConfigFile))
                {
                    ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap();

                    exeConfigurationFileMap.ExeConfigFilename = ConfigFile;

                    Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);

                    ServiceModelSectionGroup serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);

                    this.ApplySectionInfo(this.Description.ServiceType.FullName, serviceModelSectionGroup);

                    this.ApplyMultiBehaviors(serviceModelSectionGroup);
                }
            }
        }

        /// <summary>
        /// Method ApplySectionInfo.
        /// </summary>
        /// <param name="serviceFullName">The full name of the type of hosted service.</param>
        /// <param name="serviceModelSectionGroup">ServiceModel section group.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool ApplySectionInfo(string serviceFullName, ServiceModelSectionGroup serviceModelSectionGroup)
        {
            if (serviceModelSectionGroup == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(serviceFullName))
            {
                return false;
            }

            bool isElementExist = false;

            foreach (ServiceElement element in serviceModelSectionGroup.Services.Services)
            {
                if (element.Name == serviceFullName)
                {
                    this.LoadConfigurationSection(element);

                    isElementExist = true;

                    break;
                }
            }

            return isElementExist;
        }

        /// <summary>
        /// Method ApplyMultiBehaviors
        /// </summary>
        /// <param name="serviceModelSectionGroup">ServiceModel section group.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool ApplyMultiBehaviors(ServiceModelSectionGroup serviceModelSectionGroup)
        {
            if (serviceModelSectionGroup == null)
            {
                return false;
            }

            foreach (ServiceBehaviorElement element in serviceModelSectionGroup.Behaviors.ServiceBehaviors)
            {
                foreach (BehaviorExtensionElement behavior in element)
                {
                    BehaviorExtensionElement behaviorExtensionElement = behavior;

                    object extention = behaviorExtensionElement.GetType().InvokeMember(
                        "CreateBehavior",
                        BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        behaviorExtensionElement,
                        null);

                    if (extention == null)
                    {
                        continue;
                    }

                    IServiceBehavior serviceBehavior = (IServiceBehavior)extention;

                    bool isBehaviorExist = false;

                    foreach (IServiceBehavior item in this.Description.Behaviors)
                    {
                        if (item.GetType().Name == serviceBehavior.GetType().Name)
                        {
                            isBehaviorExist = true;

                            break;
                        }
                    }

                    if (isBehaviorExist)
                    {
                        break;
                    }

                    this.Description.Behaviors.Add((IServiceBehavior)extention);
                }
            }

            return true;
        }
    }
}
