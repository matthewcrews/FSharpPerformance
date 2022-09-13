Code to illustrate the performance difference between Array, Dictionary, IDictionary, and Map lookups.

```
// * Summary *

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.856/21H2)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT AVX2


|                    Method |        Mean |     Error |    StdDev |
|-------------------------- |------------:|----------:|----------:|
|               ArrayLookup |    536.1 ns |   5.13 ns |   4.55 ns |
|          DictionaryLookup |  4,118.3 ns |  50.85 ns |  45.07 ns |
|  ReadOnlyDictionaryLookup |  5,654.2 ns |  69.17 ns |  61.32 ns |
| ImmutableDictionaryLookup | 23,606.4 ns | 322.53 ns | 301.69 ns |
|                DictLookup |  9,882.9 ns |  87.49 ns |  81.84 ns |
|                 MapLookup | 32,307.9 ns | 180.76 ns | 150.94 ns |
```