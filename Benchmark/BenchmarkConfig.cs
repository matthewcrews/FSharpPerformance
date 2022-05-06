using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Benchmark
{
    public sealed class BenchmarkConfig : ManualConfig
    {
        // mrange: Been writing F# since 2010:ish, still can't
        //  get the OO parts right sometimes so C# it is
        public BenchmarkConfig()
        {
            // Use .NET 6.0 default mode:
            AddJob(Job.Default.WithId("STD"));

            // Use Dynamic PGO mode:
            AddJob(Job.Default.WithId("PGO")
                .WithEnvironmentVariables(
                    new EnvironmentVariable("DOTNET_TieredPGO"          , "1"),
                    new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "1"),
                    new EnvironmentVariable("DOTNET_ReadyToRun"         , "0")));
        }
    }
}