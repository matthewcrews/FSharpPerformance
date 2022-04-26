``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
|          Int |  2.455 μs | 0.0329 μs | 0.0292 μs |
|          UoM |  2.473 μs | 0.0135 μs | 0.0105 μs |
|       Record | 15.028 μs | 0.2775 μs | 0.2595 μs |
| StructRecord | 17.282 μs | 0.3286 μs | 0.3516 μs |
|           DU | 13.972 μs | 0.1482 μs | 0.1386 μs |
|      StuctDU | 17.839 μs | 0.3518 μs | 0.3764 μs |
|       String |  4.620 μs | 0.0532 μs | 0.0471 μs |
