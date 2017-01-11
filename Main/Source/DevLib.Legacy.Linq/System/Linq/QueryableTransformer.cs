using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq
{
    internal class QueryableTransformer : ExpressionTransformer
    {
        protected override Expression VisitConstant(ConstantExpression constant)
        {
            var qe = constant.Value as IQueryableEnumerable;
            if (qe == null)
                return constant;

            return Expression.Constant(qe.GetEnumerable());
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            return lambda;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (IsQueryableExtension(methodCall.Method))
                return ReplaceQueryableMethod(methodCall);

            return base.VisitMethodCall(methodCall);
        }

        private static Type GetComparableType(Type type)
        {
            if (type.IsGenericInstanceOf(typeof(IQueryable<>)))
                type = typeof(IEnumerable<>).MakeGenericTypeFrom(type);
            else if (type.IsGenericInstanceOf(typeof(IOrderedQueryable<>)))
                type = typeof(IOrderedEnumerable<>).MakeGenericTypeFrom(type);
            else if (type.IsGenericInstanceOf(typeof(Expression<>)))
                type = type.GetFirstGenericArgument();
            else if (type == typeof(IQueryable))
                type = typeof(IEnumerable);

            return type;
        }

        private static MethodInfo GetMatchingMethod(MethodInfo method, Type declaring)
        {
            foreach (var candidate in declaring.GetMethods())
            {
                if (!MethodMatch(candidate, method))
                    continue;

                if (method.IsGenericMethod)
                    return candidate.MakeGenericMethodFrom(method);

                return candidate;
            }

            return null;
        }

        private static Type GetTargetDeclaringType(MethodInfo method)
        {
            return method.DeclaringType == typeof(Queryable) ? typeof(Enumerable) : method.DeclaringType;
        }

        private static bool HasExtensionAttribute(MethodInfo method)
        {
            return method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length > 0;
        }

        private static bool IsQueryableExtension(MethodInfo method)
        {
            return HasExtensionAttribute(method) &&
                method.GetParameters()[0].ParameterType.IsAssignableTo(typeof(IQueryable));
        }

        private static bool MethodMatch(MethodInfo candidate, MethodInfo method)
        {
            if (candidate.Name != method.Name)
                return false;

            if (!HasExtensionAttribute(candidate))
                return false;

            var parameters = method.GetParameterTypes();

            if (parameters.Length != candidate.GetParameters().Length)
                return false;

            if (method.IsGenericMethod)
            {
                if (!candidate.IsGenericMethod)
                    return false;

                if (candidate.GetGenericArguments().Length != method.GetGenericArguments().Length)
                    return false;

                candidate = candidate.MakeGenericMethodFrom(method);
            }

            if (!TypeMatch(candidate.ReturnType, method.ReturnType))
                return false;

            var candidate_parameters = candidate.GetParameterTypes();

            if (candidate_parameters[0] != GetComparableType(parameters[0]))
                return false;

            for (int i = 1; i < candidate_parameters.Length; ++i)
                if (!TypeMatch(candidate_parameters[i], parameters[i]))
                    return false;

            return true;
        }

        private static MethodInfo ReplaceQueryableMethod(MethodInfo method)
        {
            var target_type = GetTargetDeclaringType(method);
            var result = GetMatchingMethod(method, target_type);

            if (result != null)
                return result;

            throw new InvalidOperationException(
                string.Format(
                    "There is no method {0} on type {1} that matches the specified arguments",
                    method.Name,
                    target_type.FullName));
        }

        private static bool TypeMatch(Type candidate, Type type)
        {
            if (candidate == type)
                return true;

            return candidate == GetComparableType(type);
        }

        private static Expression UnquoteIfNeeded(Expression expression, Type delegateType)
        {
            if (expression.NodeType != ExpressionType.Quote)
                return expression;

            var lambda = (LambdaExpression)((UnaryExpression)expression).Operand;
            if (lambda.Type == delegateType)
                return lambda;

            return expression;
        }

        private MethodCallExpression ReplaceQueryableMethod(MethodCallExpression old)
        {
            Expression target = null;
            if (old.Object != null)
                target = Visit(old.Object);

            var method = ReplaceQueryableMethod(old.Method);
            var parameters = method.GetParameters();
            var arguments = new Expression[old.Arguments.Count];

            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = UnquoteIfNeeded(
                    Visit(old.Arguments[i]),
                    parameters[i].ParameterType);
            }

            return Expression.Call(target, method, arguments);
        }
    }
}
