﻿using System;
using System.Collections.Generic;
using System.Linq;
using BabyKusto.Core.Statements;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core
{
    public class BabyKustoEngine
    {
        private readonly List<(string TableName, ITableSource Source)> _globalTables = new();
        private readonly Stack<ExecutionContext> _executionContexts;

        public BabyKustoEngine()
        {
            _executionContexts = new Stack<ExecutionContext>();
        }

        public void AddGlobalTable(string tableName, ITableSource source)
        {
            _globalTables.Add((tableName, source));
        }

        internal ExecutionContext ExecutionContext => _executionContexts.Peek();

        public object? Evaluate(string query)
        {
            // TODO: davidni: Set up global state somehwere proper where it would be done just once
            var db = new DatabaseSymbol(
                "MyDb",
                _globalTables.Select(
                    tableDef => new TableSymbol(
                        tableDef.TableName,
                        "(" + string.Join(",", tableDef.Source.Schema.ColumnDefinitions.Select(c => $"{c.ColumnName}: {c.ValueKind.ToString().ToLower()}")) + ")")
                ).ToArray());
            GlobalState globals = GlobalState.Default.WithDatabase(db);

            var globalObjects = _globalTables
                .Select(globalTable => new KeyValuePair<string, object?>(globalTable.TableName, globalTable.Source))
                .ToArray();
            _executionContexts.Push(new ExecutionContext(null, globalObjects));

            var code = KustoCode.ParseAndAnalyze(query, globals);
            if (false)
            {
                DumpTree(code);
            }

            var diagnostics = code.GetDiagnostics();
            if (diagnostics.Count > 0)
            {
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine($"Kusto diagnostics: {diag.Severity} {diag.Code} {diag.Message} {diag.Description}");
                }

                throw new InvalidOperationException("Query is malformed.");
            }

            if (code.Syntax is not QueryBlock queryBlock)
            {
                throw new InvalidOperationException($"Only QueryBlocks are supported as top level syntax elements, found {TypeNameHelper.GetTypeDisplayName(code.Syntax)}");
            }

            object? lastResult = null;
            foreach (var statementEntry in queryBlock.Statements)
            {
                var statement = statementEntry.Element;
                var sterlingStatement = BabyKustoStatement.Build(this, statement);
                lastResult = sterlingStatement.Execute();
            }

            return lastResult;

            static void DumpTree(KustoCode code)
            {
                int indent = 0;
                SyntaxElement.WalkNodes(
                    code.Syntax,
                    fnBefore: node =>
                    {
                        Console.Write(new string(' ', indent));
                        Console.WriteLine($"{node.Kind} ({TypeNameHelper.GetTypeDisplayName(node.GetType())}): {node.ToString(IncludeTrivia.SingleLine)}");
                        indent++;
                    },
                    fnAfter: node =>
                    {
                        indent--;
                    });

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        internal void PushDeclarationsContext(string name, object? value)
        {
            var context = new ExecutionContext(_executionContexts.Peek(), new KeyValuePair<string, object?>(name, value));
            _executionContexts.Push(context);
        }

        internal void LeaveExecutionContext()
        {
            _executionContexts.Pop();
        }
    }
}