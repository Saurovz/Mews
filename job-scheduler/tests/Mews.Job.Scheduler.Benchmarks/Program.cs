using System.Reflection;
using BenchmarkDotNet.Running;

namespace Mews.Job.Scheduler.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), args: args);
    }
}
