//-----------------------------------------------------------------------
// <copyright file="Evaluator.cs" company="YuGuan Corporation">
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
        /// Method CanBeEvaluatedLocally.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>true if the expression can be evaluated locally; otherwise, false.</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }
    }
}
