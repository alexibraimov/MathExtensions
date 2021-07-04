using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Diff
{
    public static class MathExtensions
    {
        private static readonly Dictionary<ExpressionType, Func<Expression, Expression>> derivativesFuncs;
        private static Dictionary<string, Func<MethodCallExpression, Expression>> funcsnNamesAndDiffMethods;

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
            funcsnNamesAndDiffMethods = new Dictionary<string, Func<MethodCallExpression, Expression>>
            {
                [nameof(Math.Sin)] = e => DiffSin(e),
                [nameof(Math.Cos)] = e => DifCos(e),
                [nameof(Math.Pow)] = e => DiffPow(e),
                [nameof(Math.Exp)] = e => DiffExp(e),
                [nameof(Math.Log)] = e => DiffLog(e),
                [nameof(Math.Tan)] = e => DiffTan(e),
                [nameof(Math.Asin)] = e => DiffAsin(e),
                [nameof(Math.Acos)] = e => DiffAcos(e),
                [nameof(Math.Atan)] = e => DiffAtan(e),
                [nameof(Math.Tanh)] = e => DiffTanh(e),
                [nameof(Math.Sinh)] = e => DiffSinh(e),
                [nameof(Math.Cosh)] = e => DiffCosh(e),
            };

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
            derivativesFuncs.Add(ExpressionType.Call, expression =>
            {
                var e = (MethodCallExpression)expression;
                if (funcsnNamesAndDiffMethods.ContainsKey(e.Method.Name))
                    return funcsnNamesAndDiffMethods[e.Method.Name](e);
                return null;
            });
        }

        private static Expression DiffSin(MethodCallExpression e)
        {
            return Expression.Multiply(
                        Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Cos)), e.Arguments[0]),
                        derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0])
                        );
        }

        private static Expression DifCos(MethodCallExpression e)
        {
            return Expression.Multiply(
                       Expression.Multiply(Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Sin)), e.Arguments[0]),
                       Expression.Constant(-1.0)),
                       derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
        }

        private static Expression DiffTan(MethodCallExpression e)
        {
            var cos = Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Cos)), e.Arguments[0]);
            return Expression.Divide(derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]), Expression.Multiply(cos, cos));
        }

        private static Expression DiffCosh(MethodCallExpression e)
        {
            return Expression.Multiply(
                     Expression.Call(null, typeof(Math).GetMethod((nameof(Math.Sinh))), e.Arguments[0]),
                     derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
        }

        private static Expression DiffSinh(MethodCallExpression e)
        {
            return Expression.Multiply(
                     Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Cosh)), e.Arguments[0]),
                     derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
        }

        private static Expression DiffTanh(MethodCallExpression e)
        {
            var cosh = Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Cosh)), e.Arguments[0]);
            return Expression.Divide(derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]), Expression.Multiply(cosh, cosh));
        }

        private static Expression DiffAtan(MethodCallExpression e)
        {
            return Expression.Multiply(
              Expression.Divide(Expression.Constant(1.0), Expression.Add(Expression.Constant(1.0), Expression.Multiply(e.Arguments[0], e.Arguments[0]))),
              derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0])
              );
        }

        private static Expression DiffAcos(MethodCallExpression e)
        {
            return Expression.Multiply(
              Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Pow)), Expression.Add(Expression.Constant(1.0), Expression.Multiply(Expression.Multiply(Expression.Constant(-1.0), e.Arguments[0]), e.Arguments[0])), Expression.Constant(-0.5)),
              Expression.Multiply(derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]), Expression.Constant(-1.0)));
        }

        private static Expression DiffAsin(MethodCallExpression e)
        {
            return Expression.Multiply(Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Pow)), Expression.Add(Expression.Constant(1.0), Expression.Multiply(Expression.Multiply(Expression.Constant(-1.0), e.Arguments[0]), e.Arguments[0])), Expression.Constant(-0.5)), derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
        }

        private static Expression DiffLog(MethodCallExpression e)
        {
            if (e.Arguments.Count == 2)
                return Expression.Divide(derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]), Expression.Multiply(e.Arguments[0], Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) }), e.Arguments[1])));
            else
                return Expression.Divide(derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]), e.Arguments[0]);
        }

        private static Expression DiffExp(MethodCallExpression e)
        {
            return Expression.Multiply(Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Exp)), e.Arguments[0]), derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
        }

        private static Expression DiffPow(MethodCallExpression e)
        {
            if (e.Arguments[0] is ParameterExpression && e.Arguments[1] is ConstantExpression)
                return Expression.Multiply(Expression.Multiply(e.Arguments[1], Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Pow)), e.Arguments[0],Expression.Add(e.Arguments[1], Expression.Constant(-1.0)))), derivativesFuncs[e.Arguments[0].NodeType](e.Arguments[0]));
            else if (e.Arguments[0] is ConstantExpression && e.Arguments[1] is ParameterExpression)
            {
                var internalFunc = derivativesFuncs[e.Arguments[1].NodeType](e.Arguments[1]);
                var externalFunc = Expression.Multiply(e, Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) }), e.Arguments[0]));
                return Expression.Multiply(internalFunc, externalFunc);
            }
            else
            {
                var result = Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Exp)), Expression.Multiply(e.Arguments[1], Expression.Call(null, typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) }), e.Arguments[0])));
                return derivativesFuncs[result.NodeType](result);
            }
        }
    }
}
