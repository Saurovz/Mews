using System.Reflection;
using BenchmarkDotNet.Running;

namespace TaxManager.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), args: args);
    }
}
