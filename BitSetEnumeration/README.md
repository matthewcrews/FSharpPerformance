Some examples of showing different approaches to iterate through a `BitSet` and the performance difference.

```
// * Summary *

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.819)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.402
  [Host]     : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2
```

|     Method |      Mean |     Error |    StdDev |   Gen0 | Allocated |
|----------- |----------:|----------:|----------:|-------:|----------:|
|    HashSet | 23.347 ns | 0.4024 ns | 0.3764 ns |      - |         - |
|       Iter |  7.087 ns | 0.1000 ns | 0.0935 ns |      - |         - |
| Enumerable | 55.521 ns | 0.9228 ns | 0.8181 ns | 0.0048 |      40 B |
| DuckTyping | 28.039 ns | 0.5183 ns | 0.4848 ns |      - |         - |
|   Inlining | 13.315 ns | 0.2178 ns | 0.2037 ns |      - |         - |
