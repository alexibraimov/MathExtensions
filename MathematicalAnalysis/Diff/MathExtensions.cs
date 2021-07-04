﻿using System;
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
            throw new NotImplementedException();
        }

        private static Expression DiffExp(MethodCallExpression e)
        {
            throw new NotImplementedException();
        }

        private static Expression DiffPow(MethodCallExpression e)
        {
            throw new NotImplementedException();
        }
    }
}
