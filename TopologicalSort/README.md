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

| Method |        Mean |       Error |      StdDev | BranchInstructions/Op | CacheMisses/Op | BranchMispredictions/Op |     Gen 0 | Allocated |
|------- |------------:|------------:|------------:|----------------------:|---------------:|------------------------:|----------:|----------:|
|    V01 | 55,409.9 us | 1,001.88 us |   937.16 us |           126,827,889 |        432,783 |               1,032,438 | 4800.0000 | 39,926 KB |
|    V02 | 37,327.1 us |   744.69 us | 1,987.73 us |            75,607,969 |        510,333 |                 624,307 | 2000.0000 | 21,048 KB |
|    V03 |  8,918.0 us |   166.02 us |   350.18 us |            14,307,544 |        242,670 |                 213,351 |         - |  6,142 KB |
|    V04 |  5,437.0 us |   120.52 us |   353.47 us |             6,912,737 |        150,774 |                 148,726 |         - |  6,142 KB |
|    V05 |  2,537.0 us |    50.42 us |    95.93 us |             4,166,034 |        146,332 |                 175,325 |         - |  2,766 KB |
|    V06 |  1,166.9 us |    25.05 us |    71.46 us |             2,156,790 |         75,244 |                  82,780 |         - |  1,360 KB |
|    V07 |    895.3 us |    16.64 us |    16.34 us |             1,648,230 |         63,693 |                  70,451 |         - |    845 KB |
|    V08 |    818.2 us |    16.20 us |    31.21 us |             1,431,306 |         57,754 |                  67,420 |         - |    508 KB |
|    V09 |    677.8 us |    13.13 us |    18.84 us |             1,205,862 |         44,510 |                  57,890 |         - |    235 KB |
|    V10 |    610.1 us |    12.15 us |    21.60 us |             1,035,788 |         37,963 |                  52,049 |         - |    235 KB |
|    V11 |    578.3 us |    11.27 us |    18.83 us |               921,046 |         38,635 |                  52,141 |         - |    180 KB |
|    V12 |    342.9 us |     6.77 us |     9.93 us |               524,288 |         27,392 |                  33,792 |         - |    125 KB |
|    V13 |    232.4 us |     4.63 us |     9.46 us |               431,548 |         20,480 |                  14,452 |         - |    125 KB |

## Profiling

To profile a specific method, you can use the following command.

`dotnet run -c Release --task profile --method <Method>  --iterations <Iterations>`

The value for `<Method>` corresponds to which version of Topological Sort you want to profile. Valid inputs are `V01` through `V10`.

`<Iterations>` is the number of times you want the method to be called. You want the number to be high enough that the performance is indicative of real-world usage. Ideally you want the method to be called enough that the JIT fully optimizes the code. I target at least 5 seconds as a baseline. I would start with 1,000 iterations and increase by 10x until you get a reasonable amount of runtime.

