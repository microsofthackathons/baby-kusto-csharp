// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core
{
    public class Column
    {
        private readonly object?[] _data;

        public Column(object?[] data)
        {
            _data = data;
        }

        public int RowCount => _data.Length;

        public object? this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public Span<object?> GetSpan(int start, int length)
        {
            return _data.AsSpan(start, length);
        }
    }
}
