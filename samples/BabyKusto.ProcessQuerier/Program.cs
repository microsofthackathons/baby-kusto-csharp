// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Drawing;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

Console.WriteLine(@"/----------------------------------------------------------------\");
Console.WriteLine(@"| Welcome to BabyKusto.ProcessQuerier. You can write KQL queries |");
Console.WriteLine(@"| and explore the live list of processes on your machine.        |");
Console.WriteLine(@"\----------------------------------------------------------------/");
Console.WriteLine();

ShowDemos();

while (true)
{
    try
    {
        PrintCaret();
        string query = Console.ReadLine();

        ExecuteReplQuery(query);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
    }

    Console.WriteLine("");
}

static void ShowDemos()
{
    ShowDemo1();
    ShowDemo2();
}

static void ShowDemo1()
{
    string query = @"Processes | count";

    var lastColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("Example: counting the total number of processes:");
    Console.ForegroundColor = lastColor;
    PrintCaret();
    Console.WriteLine(query);
    ExecuteReplQuery(query);
}

static void ShowDemo2()
{
    string query = @"Processes | order by numThreads desc | take 1";

    var lastColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("Example: Find the process with the most number of threads:");
    Console.ForegroundColor = lastColor;
    PrintCaret();
    Console.WriteLine(query);
    ExecuteReplQuery(query);
}

static void ExecuteReplQuery(string query)
{
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

static void PrintCaret()
{
    var lastColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("> ");
    Console.ForegroundColor = lastColor;
}