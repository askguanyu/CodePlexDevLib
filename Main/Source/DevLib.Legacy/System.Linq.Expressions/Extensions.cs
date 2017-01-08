using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
    internal static class Extensions
    {
        public static Type GetFirstGenericArgument(this Type self)
        {
            return self.GetGenericArguments()[0];
        }

        public static MethodInfo GetInvokeMethod(this Type self)
        {
            return self.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        }

        public static Type GetNotNullableType(this Type self)
        {
            return self.IsNullable() ? self.GetFirstGenericArgument() : self;
        }

        public static Type[] GetParameterTypes(this MethodBase self)
        {
            var parameters = self.GetParameters();
            var types = new Type[parameters.Length];

            for (int i = 0; i < types.Length; i++)
                types[i] = parameters[i].ParameterType;

            return types;
        }

        public static bool IsAssignableTo(this Type self, Type type)
        {
            return type.IsAssignableFrom(self) ||
                ArrayTypeIsAssignableTo(self, type);
        }

        public static bool IsExpression(this Type self)
        {
            return self == typeof(Expression) || self.IsSubclassOf(typeof(Expression));
        }

        public static bool IsGenericImplementationOf(this Type self, Type type)
        {
            foreach (Type iface in self.GetInterfaces())
                if (iface.IsGenericInstanceOf(type))
                    return true;
            return false;
        }

        public static bool IsGenericInstanceOf(this Type self, Type type)
        {
            if (!self.IsGenericType)
                return false;

            return self.GetGenericTypeDefinition() == type;
        }

        public static bool IsNullable(this Type self)
        {
            return self.IsValueType && self.IsGenericInstanceOf(typeof(Nullable<>));
        }

        public static MethodInfo MakeGenericMethodFrom(this MethodInfo self, MethodInfo method)
        {
            return self.MakeGenericMethod(method.GetGenericArguments());
        }

        public static Type MakeGenericTypeFrom(this Type self, Type type)
        {
            return self.MakeGenericType(type.GetGenericArguments());
        }

        public static Type MakeNullableType(this Type self)
        {
            return typeof(Nullable<>).MakeGenericType(self);
        }

        public static Type MakeStrongBoxType(this Type self)
        {
            return typeof(StrongBox<>).MakeGenericType(self);
        }

        public static void OnFieldOrProperty(this MemberInfo self,
            Action<FieldInfo> onfield, Action<PropertyInfo> onprop)
        {
            switch (self.MemberType)
            {
                case MemberTypes.Field:
                    onfield((FieldInfo)self);
                    return;

                case MemberTypes.Property:
                    onprop((PropertyInfo)self);
                    return;

                default:
                    throw new ArgumentException();
            }
        }

        public static T OnFieldOrProperty<T>(this MemberInfo self,
            Func<FieldInfo, T> onfield, Func<PropertyInfo, T> onprop)
        {
            switch (self.MemberType)
            {
                case MemberTypes.Field:
                    return onfield((FieldInfo)self);

                case MemberTypes.Property:
                    return onprop((PropertyInfo)self);

                default:
                    throw new ArgumentException();
            }
        }

        private static bool ArrayTypeIsAssignableTo(Type type, Type candidate)
        {
            if (!type.IsArray || !candidate.IsArray)
                return false;

            if (type.GetArrayRank() != candidate.GetArrayRank())
                return false;

            return type.GetElementType().IsAssignableTo(candidate.GetElementType());
        }
    }
}
