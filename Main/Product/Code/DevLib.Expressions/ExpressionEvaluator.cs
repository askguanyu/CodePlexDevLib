//-----------------------------------------------------------------------
// <copyright file="ExpressionEvaluator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Class ExpressionEvaluator to perform partial evaluation of an expression tree.
    /// </summary>
    public static class ExpressionEvaluator
    {
        /// <summary>
        /// Extracts the property name from a property expression.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName<T>(this Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            LambdaExpression resultExpression = propertyExpression.PartialEval() as LambdaExpression;

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
        /// <typeparam name="TResult">The type of the property to get.</typeparam>
        /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
        /// <returns>The name of the property.</returns>
        public static string ExtractPropertyName<TSource, TResult>(this Expression<Func<TSource, TResult>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            LambdaExpression resultExpression = propertyExpression.PartialEval() as LambdaExpression;

            Expression expressionBody = resultExpression.Body;

            if (resultExpression.Body is UnaryExpression)
            {
                expressionBody = ((UnaryExpression)resultExpression.Body).Operand;
            }

            string result = expressionBody.ToString().Replace(".get_Item", string.Empty).Replace('(', '[').Replace(')', ']').Substring(resultExpression.Parameters[0].Name.Length + 1);

            return result;
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(this Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeExpressionVisitor(new NominatorExpressionVisitor(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(this Expression expression)
        {
            return PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);
        }

        /// <summary>
        /// Determines whether the specified expression can be evaluated locally.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>true if the expression can be evaluated locally; otherwise, false.</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }
    }
}
