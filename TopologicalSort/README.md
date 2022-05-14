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

| Method |       Mean |    Error |   StdDev |  Gen 0 | BranchInstructions/Op | CacheMisses/Op | BranchMispredictions/Op | Allocated |
|------- |-----------:|---------:|---------:|-------:|----------------------:|---------------:|------------------------:|----------:|
|     V1 | 4,460.8 ns | 59.51 ns | 52.75 ns | 0.4730 |                 9,414 |             27 |                      30 |   3,976 B |
|     V2 | 2,739.0 ns | 38.09 ns | 33.77 ns | 0.2823 |                 5,510 |             21 |                      21 |   2,384 B |
|     V3 | 1,926.5 ns | 27.31 ns | 25.55 ns | 0.2689 |                 3,764 |             16 |                      14 |   2,264 B |
|     V4 |   952.5 ns | 18.95 ns | 41.59 ns | 0.2699 |                 1,780 |             10 |                       2 |   2,264 B |
|     V5 |   546.3 ns |  8.89 ns |  7.43 ns | 0.1030 |                 1,067 |              4 |                       1 |     864 B |
|     V6 |   275.6 ns |  3.15 ns |  2.80 ns | 0.0086 |                   693 |              1 |                       0 |      72 B |
|     V7 |   186.8 ns |  2.33 ns |  2.18 ns | 0.0391 |                   414 |              1 |                       0 |     328 B |