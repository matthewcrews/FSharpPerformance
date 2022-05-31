# Topological Sort

The following is the code I use to walk through my approach to benchmarking, profiling, and optimizing a [Kahn's Algorithm](https://en.wikipedia.org/wiki/Topological_sorting) for Topological Sort.

## Running Benchmarks

To run the benchmarks on your own machine, you will need to open a Terminal session as Admin. This is to enable the hardware counters which tell you about how well the CPU is being utilized. Update the current directory of your Terminal session using the following command:

`cd <Directory for fsproj file>`

Once there, use the following command to run the benchmarks:

`dotnet run -c Release --task benchmark`


## Results

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
```

| Method |        Mean |       Error |      StdDev |     Gen 0 | BranchInstructions/Op | BranchMispredictions/Op | CacheMisses/Op | Allocated |
|------- |------------:|------------:|------------:|----------:|----------------------:|------------------------:|---------------:|----------:|
|    V01 | 59,323.8 us | 1,161.88 us | 1,941.23 us | 4000.0000 |           130,524,046 |               1,132,772 |        599,154 | 39,926 KB |
|    V02 | 35,927.1 us |   701.35 us | 1,334.40 us | 2000.0000 |            74,829,563 |                 638,540 |        459,013 | 21,048 KB |
|    V03 |  9,292.0 us |   182.74 us |   381.44 us |         - |            14,294,267 |                 211,910 |        223,812 |  6,142 KB |
|    V04 |  5,211.5 us |   103.29 us |   279.24 us |         - |             7,032,927 |                 151,981 |        147,408 |  6,142 KB |
|    V05 |  2,487.1 us |    39.79 us |    35.27 us |         - |             3,726,814 |                 157,559 |        102,400 |  2,766 KB |
|    V06 |  1,177.5 us |    22.86 us |    30.52 us |         - |             2,194,399 |                  84,827 |         81,920 |  1,360 KB |
|    V07 |    964.2 us |    19.10 us |    29.17 us |         - |             1,619,373 |                  68,575 |         53,909 |    845 KB |
|    V08 |    776.1 us |    14.82 us |    14.55 us |         - |             1,295,300 |                  59,272 |         41,683 |    508 KB |
|    V09 |    693.7 us |    13.19 us |    12.34 us |         - |             1,164,228 |                  55,416 |         31,563 |    235 KB |
|    V10 |    591.4 us |    11.43 us |    13.61 us |         - |               980,061 |                  48,593 |         32,209 |    235 KB |

## Profiling

To profile a specific method, you can use the following command.

`dotnet run -c Release --task profile --method <Method>  --iterations <Iterations>`

The value for `<Method>` corresponds to which version of Topological Sort you want to profile. Valid inputs are `V01` through `V10`.

`<Iterations>` is the number of times you want the method to be called. You want the number to be high enough that the performance is indicative of real-world usage. Ideally you want the method to be called enough that the JIT fully optimizes the code. I target at least 5 seconds as a baseline. I would start with 1,000 iterations and increase by 10x until you get a reasonable amount of runtime.

