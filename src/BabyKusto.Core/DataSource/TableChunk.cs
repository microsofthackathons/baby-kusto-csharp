// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core
{
    public class TableChunk : ITableChunk
    {
        public TableChunk(TableSchema schema, Column[] columns)
        {
            if (schema.ColumnDefinitions.Count != columns.Length)
            {
                throw new ArgumentException($"Expected schema and columns to have the same lengths, found {schema.ColumnDefinitions.Count} and {columns.Length}.");
            }

            Schema = schema;
            Columns = columns;
        }

        public TableSchema Schema { get; }

        public Column[] Columns { get; }

        public int RowCount => Columns.Length == 0 ? 0 : Columns[0].RowCount;

        public IRow GetRow(int index)
        {
            var values = new object?[Schema.ColumnDefinitions.Count];
            for (int i = 0; i < Schema.ColumnDefinitions.Count; i++)
            {
                values[i] = Columns[i][index];
            }

            return new Row(Schema, values);
        }
    }
}
