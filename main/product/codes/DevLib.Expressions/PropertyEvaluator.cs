//-----------------------------------------------------------------------
// <copyright file="PropertyEvaluator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Class ExpressionEvaluator to perform extracts the property name from a property expression.
    /// </summary>
    public static class PropertyEvaluator
    {
        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <typeparam name="TSource">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName<TSource>(this Expression<Func<TSource, object>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            LambdaExpression resultExpression = propertyExpression.PartialEval() as LambdaExpression;

            if (resultExpression == null)
            {
                throw new ArgumentException("Parameter propertyExpression is not a lambda expression", "propertyExpression");
            }

            Expression expressionBody = resultExpression.Body;

            if (resultExpression.Body is UnaryExpression)
            {
                expressionBody = ((UnaryExpression)resultExpression.Body).Operand;
            }

            string result = expressionBody.ToString().Replace(".get_Item", string.Empty).Replace('(', '[').Replace(')', ']').Substring(resultExpression.Parameters[0].Name.Length + 1);

            return result;
        }

        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <typeparam name="TSource">The object type containing the property specified in the expression.</typeparam>
        /// <typeparam name="TResult">The property type to return.</typeparam>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName<TSource, TResult>(this Expression<Func<TSource, TResult>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            LambdaExpression resultExpression = propertyExpression.PartialEval() as LambdaExpression;

            if (resultExpression == null)
            {
                throw new ArgumentException("Parameter propertyExpression is not a lambda expression", "propertyExpression");
            }

            Expression expressionBody = resultExpression.Body;

            if (resultExpression.Body is UnaryExpression)
            {
                expressionBody = ((UnaryExpression)resultExpression.Body).Operand;
            }

            string result = expressionBody.ToString().Replace(".get_Item", string.Empty).Replace('(', '[').Replace(')', ']').Substring(resultExpression.Parameters[0].Name.Length + 1);

            return result;
        }
    }
}
