﻿namespace BabyKusto.Core
{
    public interface ITableChunk
    {
        TableSchema Schema { get; }

        Column[] Columns { get; }

        int RowCount { get; }

        IRow GetRow(int index);
    }
}