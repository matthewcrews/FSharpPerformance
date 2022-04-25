``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
|          Int |  2.457 μs | 0.0437 μs | 0.0408 μs |
|          UoM |  2.458 μs | 0.0478 μs | 0.0569 μs |
|       Record | 15.355 μs | 0.1531 μs | 0.1432 μs |
| StructRecord | 18.366 μs | 0.3626 μs | 0.5645 μs |
|           DU | 14.204 μs | 0.0938 μs | 0.0877 μs |
|      StuctDU | 18.461 μs | 0.3688 μs | 0.2879 μs |
|       String |  4.659 μs | 0.0533 μs | 0.0499 μs |
