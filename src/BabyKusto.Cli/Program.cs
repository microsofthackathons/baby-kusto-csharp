// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

Console.WriteLine("-----------------------------------------------------------------------");
Console.WriteLine("Welcome to BabyKusto, the little self-contained Kusto execution engine!");
Console.WriteLine("-----------------------------------------------------------------------");
Console.WriteLine();

while (true)
{
    try
    {
        Console.Write("> ");
        string query = Console.ReadLine();

        var processesTable = GetProcessesTable();
        var engine = new BabyKustoEngine();
        engine.AddGlobalTable("Processes", processesTable);
        var result = engine.Evaluate(query, dumpIRTree: true);

        Console.WriteLine();

        if (result is TabularResult tabularResult)
        {
            Console.WriteLine("Result:");
            tabularResult.Value.Dump(Console.Out, indent: 4);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
    }

    Console.WriteLine("");
}

static InMemoryTableSource GetProcessesTable()
{
    var names = new ColumnBuilder<string>(ScalarTypes.String);
    var numThreads = new ColumnBuilder<int>(ScalarTypes.Int);
    var workingSets = new ColumnBuilder<long>(ScalarTypes.Long);

    foreach (var p in Process.GetProcesses())
    {
        string processName;
        int processThreads;
        long workingSet;
        try
        {
            processName = p.ProcessName;
            processThreads = p.Threads.Count;
            workingSet = p.WorkingSet64;
        }
        catch { continue; }

        names.Add(processName);
        numThreads.Add(processThreads);
        workingSets.Add(workingSet);
    }

    var tableSymbol = TableSymbol.From("(name:string, numThreads:int, workingSet:long)").WithName("Processes");

    var myTable = new InMemoryTableSource(
        tableSymbol,
        new Column[] { names.ToColumn(), numThreads.ToColumn(), workingSets.ToColumn() });
    return myTable;
}