//-----------------------------------------------------------------------
// <copyright file="SubtreeEvaluator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Evaluates and replaces sub-trees when first candidate is reached (top-down).
    /// </summary>
    public class SubtreeExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Field _candidates.
        /// </summary>
        private HashSet<Expression> _candidates;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubtreeExpressionVisitor" /> class.
        /// </summary>
        /// <param name="candidates">Expression candidates.</param>
        internal SubtreeExpressionVisitor(HashSet<Expression> candidates)
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
}
