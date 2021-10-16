// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal sealed class BabyKustoCountOperator : BabyKustoOperator<CountOperator>
    {
        private readonly TableSchema _resultSchema;

        public BabyKustoCountOperator(BabyKustoEngine engine, CountOperator countOperator)
            : base(engine, countOperator)
        {
            _resultSchema = new TableSchema(
                new List<ColumnDefinition>
                {
                    new ColumnDefinition(countOperator.ResultType.Members[0].Name, KustoValueKind.Long),
                });
        }

        protected override ITableSource EvaluateTableInputInternal(ITableSource input)
        {
            return new CountResultTable(input, _resultSchema);
        }

        private class CountResultTable : ITableSource
        {
            private readonly ITableSource _input;
            private readonly TableSchema _resultSchema;

            public CountResultTable(ITableSource input, TableSchema resultSchema)
            {
                _input = input;
                _resultSchema = resultSchema;
            }

            public TableSchema Schema => _resultSchema;

            public IEnumerable<ITableChunk> GetData()
            {
                long count = 0;
                foreach (var chunk in _input.GetData())
                {
                    count += chunk.RowCount;
                }
                var resultData = new Column(new object?[] { count });
                yield return new TableChunk(_resultSchema, new[] { resultData });
            }
        }
    }
}
