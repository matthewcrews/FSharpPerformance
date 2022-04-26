``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|     Method |        Mean |     Error |      StdDev | Ratio | RatioSD |
|----------- |------------:|----------:|------------:|------:|--------:|
|      Array |    704.5 μs |   8.51 μs |     7.96 μs |  1.00 |    0.00 |
|        Row |    700.4 μs |   2.58 μs |     2.28 μs |  1.00 |    0.01 |
|        Map | 38,153.8 μs | 753.62 μs | 1,006.06 μs | 54.41 |    1.53 |
| Dictionary |  4,539.6 μs |  34.77 μs |    30.82 μs |  6.45 |    0.08 |
