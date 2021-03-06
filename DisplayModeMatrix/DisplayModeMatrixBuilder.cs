﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Hexdigits.DisplayModeMatrix.Strategies;

namespace Hexdigits.DisplayModeMatrix
{
    public partial class DisplayModeMatrixBuilder
    {
        private List<Factor> _factors = new List<Factor>();

        public EvaluateBehavior EvaluateBehavior { get; private set; }

        private Expression<Func<HttpContextBase, bool>> _precondition = null;

        public DisplayModeMatrixBuilder Precondition(Expression<Func<HttpContextBase, bool>> precondition)
        {
            if (_precondition != null)
            {
                throw new InvalidOperationException("precondition already existed.");
            }

            _precondition = precondition;

            return this;
        }

        public DisplayModeMatrixBuilder SetEvaluateBehavior(EvaluateBehavior behavior)
        {
            EvaluateBehavior = behavior;

            return this;
        }

        public DisplayModeMatrixBuilder AddOptionalFactor(string name, Action<FactorBuilder> register)
        {
            return AddFactor(name, false, register);
        }

        public DisplayModeMatrixBuilder AddRequiredFactor(string name, Action<FactorBuilder> register)
        {
            return AddFactor(name, true, register);
        }

        public DisplayModeMatrixBuilder AddFactor(string name, bool required, Action<FactorBuilder> register)
        {
            var factorBuilder = new FactorBuilder(_factors.Count + 1);

            register(factorBuilder);

            var hierarchy = new Factor()
            {
                Key = name,
                Required = required,
                Values = factorBuilder.Evidences
            };

            _factors.Add(hierarchy);

            return this;
        }

        public IEnumerable<DisplayModeProfile> Build()
        {
            var weight = _factors.Count;
            var strategy = EvaluateStrategyFactory.Create(EvaluateBehavior);
            var parameter = Expression.Parameter(typeof(HttpContextBase), "x");

            foreach (var factor in _factors)
            {
                foreach (var value in factor.Values)
                {
                    value.Weight = weight;
                    value.Expression = strategy.WarpExpression(value.Expression, parameter);
                }
                weight--;
            }

            var sequence = 0;

            return _factors
                        .Permutation(strategy)
                        .Where(x => x != null)
                        .OrderByDescending(x => x.Weight)
                        .Distinct()
                        .Select(x =>
                        {
                            var expression = x.Expression;

                            if (_precondition != null)
                            {
                                var body = Expression.AndAlso(
                                                Expression.Invoke(strategy.WarpExpression(_precondition, parameter), parameter),
                                                Expression.Invoke(x.Expression, parameter));

                                expression = Expression.Lambda<Func<HttpContextBase, bool>>(body, parameter);
                            }

                            if (sequence++ == 0)
                            {
                                expression = strategy.InitializePadding(expression, parameter);
                            }

                            Debug.WriteLine($"{x.Name}, {expression.Reduce()}");

                            return new DisplayModeProfile
                            {
                                Name = x.Name,
                                ContextCondition = expression.Compile()
                            };
                        });
        }
    }
}