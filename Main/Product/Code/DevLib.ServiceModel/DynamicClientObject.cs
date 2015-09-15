//-----------------------------------------------------------------------
// <copyright file="DynamicClientObject.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents the object for service client.
    /// </summary>
    [Serializable]
    public class DynamicClientObject : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientObject" /> class.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        public DynamicClientObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            this.ObjectInstance = obj;
            this.ObjectType = obj.GetType();
            this.InvokeAttr = BindingFlags.Instance | BindingFlags.Public;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientObject" /> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public DynamicClientObject(Type objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            this.ObjectType = objectType;
            this.InvokeAttr = BindingFlags.Instance | BindingFlags.Public;
        }

        /// <summary>
        /// Gets the type of object.
        /// </summary>
        public Type ObjectType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the instance of object.
        /// </summary>
        public object ObjectInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets BindingFlags for invoke attribute.
        /// </summary>
        public BindingFlags InvokeAttr
        {
            get;
            set;
        }

        /// <summary>
        /// Invokes Constructor of the object.
        /// </summary>
        public void CallConstructor()
        {
            this.CallConstructor(Type.EmptyTypes, new object[0]);
        }

        /// <summary>
        ///  Invokes Constructor of the object.
        /// </summary>
        /// <param name="paramTypes">Constructor parameter types.</param>
        /// <param name="paramValues">Constructor parameter values.</param>
        public void CallConstructor(Type[] paramTypes, object[] paramValues)
        {
            ConstructorInfo ctor = this.ObjectType.GetConstructor(paramTypes);

            if (ctor == null)
            {
                throw new ArgumentException(DynamicClientProxyConstants.ProxyCtorNotFound, "paramTypes");
            }

            this.ObjectInstance = ctor.Invoke(paramValues);
        }

        /// <summary>
        /// Gets property value by name.
        /// </summary>
        /// <param name="propertyName">Property name to get.</param>
        /// <returns>Property value.</returns>
        public object GetProperty(string propertyName)
        {
            object result = this.ObjectType.InvokeMember(
                propertyName,
                BindingFlags.GetProperty | this.InvokeAttr,
                null /* Binder */,
                this.ObjectInstance,
                null /* args */);

            return result;
        }

        /// <summary>
        /// Sets property value by name.
        /// </summary>
        /// <param name="propertyName">Property name to set.</param>
        /// <param name="value">Property value.</param>
        /// <returns>Null value.</returns>
        public object SetProperty(string propertyName, object value)
        {
            object result = this.ObjectType.InvokeMember(
                propertyName,
                BindingFlags.SetProperty | this.InvokeAttr,
                null /* Binder */,
                this.ObjectInstance,
                new object[] { value });

            return result;
        }

        /// <summary>
        /// Gets field value by name.
        /// </summary>
        /// <param name="fieldName">Field name to get.</param>
        /// <returns>Field value.</returns>
        public object GetField(string fieldName)
        {
            object result = this.ObjectType.InvokeMember(
                fieldName,
                BindingFlags.GetField | this.InvokeAttr,
                null /* Binder */,
                this.ObjectInstance,
                null /* args */);

            return result;
        }

        /// <summary>
        /// Sets field value by name.
        /// </summary>
        /// <param name="fieldName">Field name to set.</param>
        /// <param name="value">Field value.</param>
        /// <returns>Null value.</returns>
        public object SetField(string fieldName, object value)
        {
            object result = this.ObjectType.InvokeMember(
                fieldName,
                BindingFlags.SetField | this.InvokeAttr,
                null /* Binder */,
                this.ObjectInstance,
                new object[] { value });

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public object CallMethod(string methodName, params object[] parameters)
        {
            object result = this.ObjectType.InvokeMember(
                methodName,
                BindingFlags.InvokeMethod | this.InvokeAttr,
                null /* Binder */,
                this.ObjectInstance,
                parameters /* args */);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public object CallMethod(string methodName, Type[] types, object[] parameters)
        {
            if (types.Length != parameters.Length)
            {
                throw new ArgumentException(DynamicClientProxyConstants.ParameterValueMismatch);
            }

            MethodInfo methodInfo = this.ObjectType.GetMethod(methodName, types);

            if (methodInfo == null)
            {
                throw new ArgumentException(string.Format(DynamicClientProxyConstants.MethodNotFoundStringFormat, methodName), "methodName");
            }

            object result = methodInfo.Invoke(this.ObjectInstance, this.InvokeAttr, null, parameters, null);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public object CallMethod(MethodInfo methodInfo, params object[] parameters)
        {
            return methodInfo.Invoke(this.ObjectInstance, this.InvokeAttr, null, parameters, null);
        }
    }
}
