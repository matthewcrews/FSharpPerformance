Code to illustrate the performance difference between Array, Dictionary, IDictionary, and Map lookups.

```
// * Summary *

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.856/21H2)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX2


|           Method |        Mean |     Error |    StdDev |
|----------------- |------------:|----------:|----------:|
|      ArrayLookup |    522.6 ns |   2.80 ns |   2.62 ns |
| DictionaryLookup |  4,007.6 ns |  21.00 ns |  19.64 ns |
|       DictLookup |  9,723.4 ns |  27.59 ns |  25.81 ns |
|        MapLookup | 33,333.2 ns | 235.59 ns | 196.73 ns |
```