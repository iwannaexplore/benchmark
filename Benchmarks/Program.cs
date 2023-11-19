using BenchmarkDotNet.Running;
using Benchmarks;

var summary = BenchmarkRunner.Run<MyBenchmark>();
Console.WriteLine(summary);