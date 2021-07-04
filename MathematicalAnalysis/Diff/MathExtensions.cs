using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Diff
{
    public static class MathExtensions
    {
        private static readonly Dictionary<ExpressionType, Func<Expression, Expression>> derivativesFuncs;

        /// <summary>
        /// Function derivative
        /// </summary>
        /// <param name="function">Function to be differentiated</param>
        /// <returns>Differentiated function</returns>
        public static Expression<Func<double, double>> Derivative(Expression<Func<double, double>> function)
        {
            return Expression.Lambda<Func<double, double>>(derivativesFuncs[function.Body.NodeType](function.Body), function.Parameters);
        }

        static MathExtensions()
        {
            derivativesFuncs = new Dictionary<ExpressionType, Func<Expression, Expression>>();
            derivativesFuncs.Add(ExpressionType.Constant, expression => Expression.Constant(0.0));
            derivativesFuncs.Add(ExpressionType.Parameter, expression => Expression.Constant(1.0));
            derivativesFuncs.Add(ExpressionType.Add, expression =>
            {
                var e = (BinaryExpression)expression;
                return Expression.Add(derivativesFuncs[e.Left.NodeType](e.Left), derivativesFuncs[e.Right.NodeType](e.Right));
            });
            derivativesFuncs.Add(ExpressionType.Multiply, expression =>
            {
                var e = (BinaryExpression)expression;
                return Expression.Add(Expression.Multiply(e.Left, derivativesFuncs[e.Right.NodeType](e.Right)),
                                      Expression.Multiply(e.Right, derivativesFuncs[e.Left.NodeType](e.Left)));
            });
            derivativesFuncs.Add(ExpressionType.Divide, expression =>
            {
                var e = (BinaryExpression)expression;
                return Expression.Divide
                    (
                      Expression.Add(Expression.Multiply(e.Right, derivativesFuncs[e.Left.NodeType](e.Left)),
                                     Expression.Multiply(Expression.Multiply(e.Left, Expression.Constant(-1.0)),
                                                      derivativesFuncs[e.Right.NodeType](e.Right))),
                      Expression.Multiply(e.Right, e.Right)
                    );
            });
        }
    }
}
