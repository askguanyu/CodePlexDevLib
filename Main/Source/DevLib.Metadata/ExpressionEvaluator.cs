//-----------------------------------------------------------------------
// <copyright file="ExpressionEvaluator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Class ExpressionEvaluator to perform partial evaluation of an expression tree.
    /// </summary>
    internal static class ExpressionEvaluator
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
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(this Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
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

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        private class SubtreeEvaluator : ExpressionVisitor
        {
            /// <summary>
            /// Field _candidates.
            /// </summary>
            private HashSet<Expression> _candidates;

            /// <summary>
            /// Initializes a new instance of the <see cref="SubtreeEvaluator" /> class.
            /// </summary>
            /// <param name="candidates">Expression candidates.</param>
            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this._candidates = candidates;
            }

            /// <summary>
            /// Dispatches the expression to one of the more specialized visit methods in this class.
            /// </summary>
            /// <param name="expression">The expression to visit.</param>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
            public override Expression Visit(Expression expression)
            {
                if (expression == null)
                {
                    return null;
                }

                if (this._candidates.Contains(expression))
                {
                    return this.Evaluate(expression);
                }

                return base.Visit(expression);
            }

            /// <summary>
            /// Dispatches the expression to one of the more specialized visit methods in this class.
            /// </summary>
            /// <param name="expression">The expression to visit.</param>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
            internal Expression Eval(Expression expression)
            {
                return this.Visit(expression);
            }

            /// <summary>
            /// Evaluates expression.
            /// </summary>
            /// <param name="expression">The expression to evaluate.</param>
            /// <returns>Evaluated expression.</returns>
            private Expression Evaluate(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Constant)
                {
                    return expression;
                }

                LambdaExpression lambda = Expression.Lambda(expression);

                Delegate fn = lambda.Compile();

                return Expression.Constant(fn.DynamicInvoke(null), expression.Type);
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly be part of an evaluated sub-tree.
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            /// <summary>
            /// Field _fnCanBeEvaluated.
            /// </summary>
            private Func<Expression, bool> _fnCanBeEvaluated;

            /// <summary>
            /// Field _candidates.
            /// </summary>
            private HashSet<Expression> _candidates;

            /// <summary>
            /// Field _cannotBeEvaluated.
            /// </summary>
            private bool _cannotBeEvaluated;

            /// <summary>
            /// Initializes a new instance of the <see cref="Nominator" /> class.
            /// </summary>
            /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this._fnCanBeEvaluated = fnCanBeEvaluated;
            }

            /// <summary>
            /// Dispatches the expression to one of the more specialized visit methods in this class.
            /// </summary>
            /// <param name="expression">The expression to visit.</param>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this._cannotBeEvaluated;

                    this._cannotBeEvaluated = false;

                    base.Visit(expression);

                    if (!this._cannotBeEvaluated)
                    {
                        if (this._fnCanBeEvaluated(expression))
                        {
                            this._candidates.Add(expression);
                        }
                        else
                        {
                            this._cannotBeEvaluated = true;
                        }
                    }

                    this._cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }

            /// <summary>
            /// Method Nominate.
            /// </summary>
            /// <param name="expression">The expression to check.</param>
            /// <returns>Expression candidates.</returns>
            internal HashSet<Expression> Nominate(Expression expression)
            {
                this._candidates = new HashSet<Expression>();

                this.Visit(expression);

                return this._candidates;
            }
        }
    }
}
