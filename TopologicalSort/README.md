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

| Method |      Mean |     Error |    StdDev | BranchInstructions/Op | CacheMisses/Op | BranchMispredictions/Op |     Gen 0 | Allocated |
|------- |----------:|----------:|----------:|----------------------:|---------------:|------------------------:|----------:|----------:|
|     V1 | 55.526 ms | 1.0738 ms | 1.1489 ms |           120,524,345 |        461,483 |               1,108,651 | 4000.0000 |     39 MB |
|     V2 | 32.096 ms | 0.4048 ms | 0.3786 ms |            63,990,989 |        328,943 |                 541,389 | 2562.5000 |     21 MB |
|     V3 |  8.711 ms | 0.1728 ms | 0.1617 ms |            12,946,159 |        111,330 |                 154,598 |  750.0000 |      6 MB |
|     V4 |  4.196 ms | 0.0701 ms | 0.0655 ms |             6,154,684 |         69,459 |                 110,869 |  750.0000 |      6 MB |
|     V5 |  2.318 ms | 0.0432 ms | 0.0404 ms |             3,001,754 |         23,670 |                 112,627 |  335.9375 |      3 MB |
|     V6 |  1.192 ms | 0.0175 ms | 0.0146 ms |             1,634,961 |         14,914 |                  55,087 |  166.0156 |      1 MB |