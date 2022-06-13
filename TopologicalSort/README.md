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

| Method |        Mean |       Error |      StdDev |      Median |     Gen 0 | CacheMisses/Op | BranchInstructions/Op | BranchMispredictions/Op | Allocated |
|------- |------------:|------------:|------------:|------------:|----------:|---------------:|----------------------:|------------------------:|----------:|
|    V01 | 55,796.3 us | 1,110.35 us | 1,695.62 us | 56,284.1 us | 4000.0000 |        617,835 |           130,818,312 |               1,144,502 | 39,926 KB |
|    V02 | 32,479.9 us |   258.42 us |   241.72 us | 32,438.2 us | 2562.5000 |        315,529 |            70,222,370 |                 541,696 | 21,047 KB |
|    V03 |  9,264.4 us |   184.11 us |   363.41 us |  9,200.4 us |         - |        266,650 |            14,602,732 |                 227,164 |  6,143 KB |
|    V04 |  5,236.5 us |   126.58 us |   369.24 us |  5,179.4 us |         - |        156,631 |             6,980,895 |                 149,258 |  6,142 KB |
|    V05 |  2,603.2 us |    50.32 us |    70.54 us |  2,602.2 us |         - |        139,793 |             3,987,126 |                 170,314 |  2,766 KB |
|    V06 |  1,196.4 us |    28.75 us |    82.48 us |  1,182.4 us |         - |         76,431 |             2,161,377 |                  83,681 |  1,360 KB |
|    V07 |    931.8 us |    19.42 us |    54.78 us |    913.9 us |         - |         72,212 |             1,764,229 |                  73,810 |    844 KB |
|    V08 |    676.1 us |    13.21 us |    21.71 us |    675.3 us |         - |         44,613 |             1,200,903 |                  57,676 |    234 KB |
|    V09 |    612.9 us |    12.11 us |    20.88 us |    608.9 us |         - |         39,861 |             1,051,773 |                  51,650 |    235 KB |
|    V10 |    589.6 us |    11.48 us |    13.66 us |    593.5 us |         - |         34,549 |               911,805 |                  49,152 |    180 KB |
|    V11 |    340.2 us |     6.78 us |    10.35 us |    341.2 us |         - |         27,183 |               524,288 |                  33,140 |    126 KB |
|    V12 |    235.6 us |     4.68 us |    12.67 us |    232.4 us |         - |         23,959 |               453,819 |                  14,270 |    126 KB |

## Profiling

To profile a specific method, you can use the following command.

`dotnet run -c Release --task profile --method <Method>  --iterations <Iterations>`

The value for `<Method>` corresponds to which version of Topological Sort you want to profile. Valid inputs are `V01` through `V10`.

`<Iterations>` is the number of times you want the method to be called. You want the number to be high enough that the performance is indicative of real-world usage. Ideally you want the method to be called enough that the JIT fully optimizes the code. I target at least 5 seconds as a baseline. I would start with 1,000 iterations and increase by 10x until you get a reasonable amount of runtime.

