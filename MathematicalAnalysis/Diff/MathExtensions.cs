using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Diff
{
    public static class MathExtensions
    {
        private static readonly Dictionary<ExpressionType, Func<Expression, Expression>> differentiatedFuncs;

        /// <summary>
        /// Function differentiation
        /// </summary>
        /// <param name="function">Function to be differentiated</param>
        /// <returns>Differentiated function</returns>
        public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> function)
        {
            return Expression.Lambda<Func<double, double>>(differentiatedFuncs[function.Body.NodeType](function.Body), function.Parameters);
        }
    }
}
