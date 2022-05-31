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

| Method |        Mean |       Error |      StdDev |      Median | BranchInstructions/Op | CacheMisses/Op | BranchMispredictions/Op |     Gen 0 | Allocated |
|------- |------------:|------------:|------------:|------------:|----------------------:|---------------:|------------------------:|----------:|----------:|
|    V01 | 57,574.2 us | 1,126.47 us | 1,106.34 us | 57,722.2 us |           135,069,696 |        543,081 |               1,155,795 | 4000.0000 | 39,926 KB |
|    V02 | 36,548.7 us |   713.19 us | 1,131.19 us | 36,846.7 us |            74,620,446 |        517,903 |                 655,360 | 2000.0000 | 21,048 KB |
|    V03 |  9,005.6 us |   178.18 us |   325.81 us |  8,895.9 us |            14,568,355 |        247,808 |                 219,695 |         - |  6,142 KB |
|    V04 |  5,241.5 us |   114.84 us |   335.00 us |  5,121.3 us |             6,878,659 |        134,349 |                 145,531 |         - |  6,143 KB |
|    V05 |  2,459.7 us |    42.06 us |    39.34 us |  2,466.0 us |             3,656,909 |         93,662 |                 152,371 |         - |  2,766 KB |
|    V06 |  1,216.2 us |    23.38 us |    50.82 us |  1,206.3 us |             2,352,449 |         87,789 |                  92,619 |         - |  1,360 KB |
|    V07 |    931.2 us |    17.96 us |    15.93 us |    934.8 us |             1,568,495 |         51,063 |                  67,174 |         - |    845 KB |
|    V08 |    817.1 us |    16.15 us |    29.11 us |    812.0 us |             1,397,593 |         53,439 |                  63,631 |         - |    508 KB |
|    V09 |    700.4 us |    13.74 us |    16.88 us |    697.5 us |             1,182,379 |         41,643 |                  58,880 |         - |    235 KB |
|    V10 |    630.4 us |    12.26 us |    13.63 us |    633.3 us |               985,889 |         30,631 |                  49,508 |         - |    235 KB |

## Profiling

To profile one methods you will need to build the 

`dotnet run -c Release --task profile --method <Method>  --iterations <Iterations>`

The value for `<Method>` corresponds to which version of Topological Sort you want to profile. Valid inputs are `V01` through `V10`.

`<Iterations>` is the number of times you want the method to be called. You want the number to be high enough that performance is indicative of real-world usage. Ideally you want the method to be called enough that the JIT fully optimizes the code. I target at least 5 seconds as a minimum baseline. I would start with 1,000 iterations and increase by 10x until you get a reasonable amount of runtime.

