//-----------------------------------------------------------------------
// <copyright file="Nominator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Performs bottom-up analysis to determine which nodes can possibly be part of an evaluated sub-tree.
    /// </summary>
    public class NominatorExpressionVisitor : ExpressionVisitor
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
        /// Initializes a new instance of the <see cref="NominatorExpressionVisitor" /> class.
        /// </summary>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        internal NominatorExpressionVisitor(Func<Expression, bool> fnCanBeEvaluated)
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
