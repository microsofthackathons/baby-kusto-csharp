// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal sealed class BabyKustoTakeOperator : BabyKustoOperator<TakeOperator>
    {
        private readonly BabyKustoExpression _builtExpression;

        public BabyKustoTakeOperator(BabyKustoEngine engine, TakeOperator takeOperator)
            : base(engine, takeOperator)
        {
            _builtExpression = BabyKustoExpression.Build(engine, takeOperator.Expression);
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            var count = Convert.ToInt64(_builtExpression.Evaluate(input));
            return new TakeResultTable(input, count);
        }

        private class TakeResultTable : ITableSource
        {
            private readonly ITableSource _input;
            private readonly long _count;

            public TakeResultTable(ITableSource input, long count)
            {
                _input = input;
                _count = count;
            }

            public TableSchema Schema => _input.Schema;

            public IEnumerable<ITableChunk> GetData()
            {
                if (_count == 0)
                {
                    yield break;
                }

                var remaining = _count;
                foreach (var chunk in _input.GetData())
                {
                    if (remaining >= chunk.RowCount)
                    {
                        remaining -= chunk.RowCount;
                        yield return chunk;
                    }
                    else
                    {
                        var columns = new Column[chunk.Columns.Length];
                        for (int i = 0; i < columns.Length; i++)
                        {
                            columns[i] = new Column(chunk.Columns[i].GetSpan(0, (int)remaining).ToArray());
                        }
                        var trimmedChunk = new TableChunk(_input.Schema, columns);
                        remaining = 0;
                        yield return trimmedChunk;
                    }

                    if (remaining == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
