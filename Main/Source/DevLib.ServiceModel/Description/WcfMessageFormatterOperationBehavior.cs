//-----------------------------------------------------------------------
// <copyright file="WcfMessageFormatterOperationBehavior.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using DevLib.ServiceModel.Dispatcher;

    /// <summary>
    /// WcfMessageFormatter OperationBehavior
    /// </summary>
    public class WcfMessageFormatterOperationBehavior : Attribute, IOperationBehavior
    {
        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="operationDescription">The operation description.</param>
        /// <param name="bindingParameters">The binding parameters.</param>
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Applies the client behavior.
        /// </summary>
        /// <param name="operationDescription">The operation description.</param>
        /// <param name="clientOperation">The client operation.</param>
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new WcfMessageFormatter(clientOperation.Formatter);
        }

        /// <summary>
        /// Applies the dispatch behavior.
        /// </summary>
        /// <param name="operationDescription">The operation description.</param>
        /// <param name="dispatchOperation">The dispatch operation.</param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Formatter = new WcfMessageFormatter(dispatchOperation.Formatter);
        }

        /// <summary>
        /// Validates the specified operation description.
        /// </summary>
        /// <param name="operationDescription">The operation description.</param>
        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
