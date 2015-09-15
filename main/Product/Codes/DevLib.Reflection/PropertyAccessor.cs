//-----------------------------------------------------------------------
// <copyright file="PropertyAccessor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Class PropertyAccessor is used to get or set the property value.
    /// </summary>
    public class PropertyAccessor
    {
        /// <summary>
        /// Field ValueTypeOpCodeDictionary.
        /// </summary>
        private static readonly Dictionary<Type, OpCode> ValueTypeOpCodeDictionary;

        /// <summary>
        /// Field _getFunctionDictionary.
        /// </summary>
        private Dictionary<string, GetFunction<object, object>> _getFunctionDictionary;

        /// <summary>
        /// Field _setActionDictionary.
        /// </summary>
        private Dictionary<string, SetAction<object, object>> _setActionDictionary;

        /// <summary>
        /// Initializes static members of the <see cref="PropertyAccessor" /> class.
        /// </summary>
        static PropertyAccessor()
        {
            ValueTypeOpCodeDictionary = new Dictionary<Type, OpCode>(12);

            ValueTypeOpCodeDictionary[typeof(sbyte)] = OpCodes.Ldind_I1;
            ValueTypeOpCodeDictionary[typeof(byte)] = OpCodes.Ldind_U1;
            ValueTypeOpCodeDictionary[typeof(char)] = OpCodes.Ldind_U2;
            ValueTypeOpCodeDictionary[typeof(short)] = OpCodes.Ldind_I2;
            ValueTypeOpCodeDictionary[typeof(ushort)] = OpCodes.Ldind_U2;
            ValueTypeOpCodeDictionary[typeof(int)] = OpCodes.Ldind_I4;
            ValueTypeOpCodeDictionary[typeof(uint)] = OpCodes.Ldind_U4;
            ValueTypeOpCodeDictionary[typeof(long)] = OpCodes.Ldind_I8;
            ValueTypeOpCodeDictionary[typeof(ulong)] = OpCodes.Ldind_I8;
            ValueTypeOpCodeDictionary[typeof(bool)] = OpCodes.Ldind_I1;
            ValueTypeOpCodeDictionary[typeof(double)] = OpCodes.Ldind_R8;
            ValueTypeOpCodeDictionary[typeof(float)] = OpCodes.Ldind_R4;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="ignoreCase">true to ignore case of property name; otherwise, false.</param>
        public PropertyAccessor(Type targetType, bool ignoreCase = true)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            this.TargetType = targetType;

            this._getFunctionDictionary = new Dictionary<string, GetFunction<object, object>>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

            this._setActionDictionary = new Dictionary<string, SetAction<object, object>>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

            foreach (var item in this.TargetType.GetProperties())
            {
                if (item.GetIndexParameters().Length == 0)
                {
                    this._getFunctionDictionary[item.Name] = this.CreateGetFunction(this.TargetType.GetMethod(string.Format("get_{0}", item.Name)));

                    this._setActionDictionary[item.Name] = this.CreateSetAction(this.TargetType.GetMethod(string.Format("set_{0}", item.Name)));
                }
            }
        }

        /// <summary>
        /// Encapsulates a method that has one parameter and returns a value of the type specified by the <typeparamref name="TResult" /> parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
        /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
        /// <param name="arg">The parameter of the method that this delegate encapsulates.</param>
        /// <returns>The return value of the method that this delegate encapsulates.</returns>
        private delegate TResult GetFunction<T, TResult>(T arg);

        /// <summary>
        /// Encapsulates a method that has two parameters and does not return a value.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
        /// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
        /// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
        private delegate void SetAction<T1, T2>(T1 arg1, T2 arg2);

        /// <summary>
        /// Gets target type.
        /// </summary>
        public Type TargetType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the property value of the given object.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="propertyName">The property name to get.</param>
        /// <returns>The property value of the given object.</returns>
        public object GetProperty(object targetObject, string propertyName)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException("targetObject");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.CheckType(targetObject, "targetObject");

            return this._getFunctionDictionary[propertyName](targetObject);
        }

        /// <summary>
        /// Sets the property value of the given object.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="propertyName">The property name to set.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(object targetObject, string propertyName, object value)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException("targetObject");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.CheckType(targetObject, "targetObject");

            this._setActionDictionary[propertyName](targetObject, value);
        }

        /// <summary>
        /// Method CheckType.
        /// </summary>
        /// <param name="value">The object to check.</param>
        /// <param name="parameterName">The parameter name.</param>
        private void CheckType(object value, string parameterName)
        {
            if (value != null && !this.TargetType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException("The target type cannot be assignable from the type of given object.", parameterName);
            }
        }

        /// <summary>
        /// Method CreateGetFunction.
        /// </summary>
        /// <param name="getMethod">MethodInfo of get method.</param>
        /// <returns>GetFunction delegate.</returns>
        private GetFunction<object, object> CreateGetFunction(MethodInfo getMethod)
        {
            DynamicMethod method = new DynamicMethod("GetValue", typeof(object), new Type[] { typeof(object) });

            ILGenerator ilGenerator = method.GetILGenerator();

            ilGenerator.DeclareLocal(typeof(object));
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, this.TargetType);
            ilGenerator.EmitCall(OpCodes.Call, getMethod, null);

            if (getMethod.ReturnType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, getMethod.ReturnType);
            }

            ilGenerator.Emit(OpCodes.Stloc_0);
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ret);

            method.DefineParameter(1, ParameterAttributes.In, "value");

            return (GetFunction<object, object>)method.CreateDelegate(typeof(GetFunction<object, object>));
        }

        /// <summary>
        /// Method CreateSetAction.
        /// </summary>
        /// <param name="setMethod">MethodInfo of set method.</param>
        /// <returns>SetAction delegate.</returns>
        private SetAction<object, object> CreateSetAction(MethodInfo setMethod)
        {
            Type paramType = setMethod.GetParameters()[0].ParameterType;

            DynamicMethod method = new DynamicMethod("SetValue", null, new Type[] { typeof(object), typeof(object) });

            ILGenerator ilGenerator = method.GetILGenerator();

            ilGenerator.DeclareLocal(paramType);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, this.TargetType);
            ilGenerator.Emit(OpCodes.Ldarg_1);

            if (paramType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox, paramType);
                if (ValueTypeOpCodeDictionary.ContainsKey(paramType))
                {
                    OpCode load = (OpCode)ValueTypeOpCodeDictionary[paramType];
                    ilGenerator.Emit(load);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldobj, paramType);
                }
            }
            else
            {
                ilGenerator.Emit(OpCodes.Castclass, paramType);
            }

            ilGenerator.EmitCall(OpCodes.Callvirt, setMethod, null);
            ilGenerator.Emit(OpCodes.Ret);

            method.DefineParameter(1, ParameterAttributes.In, "obj");
            method.DefineParameter(2, ParameterAttributes.In, "value");

            return (SetAction<object, object>)method.CreateDelegate(typeof(SetAction<object, object>));
        }
    }

    /// <summary>
    /// Class PropertyAccessor is used to get or set the property value.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class PropertyAccessor<T> : PropertyAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor{T}"/> class.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case of property name; otherwise, false.</param>
        public PropertyAccessor(bool ignoreCase = true)
            : base(typeof(T), ignoreCase)
        {
        }
    }
}
