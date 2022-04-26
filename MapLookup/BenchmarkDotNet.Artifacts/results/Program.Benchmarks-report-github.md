``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
|          Int |  2.459 μs | 0.0298 μs | 0.0279 μs |
|          UoM |  2.445 μs | 0.0237 μs | 0.0210 μs |
|       Record | 14.392 μs | 0.2327 μs | 0.2176 μs |
| StructRecord | 17.773 μs | 0.3274 μs | 0.5820 μs |
|           DU | 14.920 μs | 0.2275 μs | 0.2017 μs |
|      StuctDU | 17.354 μs | 0.3428 μs | 0.3947 μs |
|       String |  4.642 μs | 0.0244 μs | 0.0204 μs |
