# Topological Sort

The following is the code I use to walk through my approach to benchmarking, profiling, and optimizing a [Kahn's Algorithm](https://en.wikipedia.org/wiki/Topological_sorting) for Topological Sort.

## Results

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
```

| Method |      Mean |     Error |    StdDev | BranchInstructions/Op | CacheMisses/Op | BranchMispredictions/Op |     Gen 0 |   Gen 1 | Allocated |
|------- |----------:|----------:|----------:|----------------------:|---------------:|------------------------:|----------:|--------:|----------:|
|     V1 | 53.782 ms | 0.8550 ms | 0.7140 ms |           114,054,048 |        422,516 |                 998,796 | 4800.0000 |       - |     39 MB |
|     V2 | 13.659 ms | 0.2652 ms | 0.2724 ms |            22,143,277 |        117,538 |                 274,104 | 1515.6250 | 15.6250 |     12 MB |
|     V3 |  6.805 ms | 0.1355 ms | 0.1899 ms |             9,957,482 |         74,862 |                 168,675 | 1484.3750 | 23.4375 |     12 MB |
|     V4 |  4.782 ms | 0.0935 ms | 0.1148 ms |             6,931,875 |         43,599 |                 195,174 |  882.8125 |       - |      7 MB |
|     V5 |  3.332 ms | 0.0272 ms | 0.0254 ms |             5,503,130 |         30,839 |                 130,881 |  816.4063 |  3.9063 |      7 MB |