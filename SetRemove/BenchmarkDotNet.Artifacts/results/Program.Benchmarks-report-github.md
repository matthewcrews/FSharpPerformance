``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|       Method |     Mean |     Error |    StdDev |   Median |
|------------- |---------:|----------:|----------:|---------:|
|          Int | 1.468 μs | 0.0998 μs | 0.2943 μs | 1.319 μs |
|          UoM | 1.413 μs | 0.0909 μs | 0.2680 μs | 1.308 μs |
|       Record | 3.003 μs | 0.1476 μs | 0.4211 μs | 2.854 μs |
| StructRecord | 3.586 μs | 0.2262 μs | 0.6669 μs | 3.189 μs |
|           DU | 3.246 μs | 0.2006 μs | 0.5916 μs | 2.932 μs |
|     StructDU | 3.574 μs | 0.2046 μs | 0.6031 μs | 3.297 μs |
