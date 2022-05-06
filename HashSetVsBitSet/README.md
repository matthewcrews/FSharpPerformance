# Faster than HashSet

I have an interesting challenge where HashSet operations are the bottleneck for my simulation. I decided to see if I could go faster using an `int64 array` as the backing store and bit operations for updating. It appears that you can if you can make assumptions about your domain.

## Results

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
```


|           Method |         Mean |        Error |       StdDev |  Gen 0 |  Gen 1 | Allocated |
|----------------- |-------------:|-------------:|-------------:|-------:|-------:|----------:|
|       HashSetAdd |    129.45 ns |     2.287 ns |     3.206 ns |      - |      - |         - |
|        BitSetAdd |     84.67 ns |     1.521 ns |     1.348 ns |      - |      - |         - |
|    BitSetAddMany |     72.62 ns |     1.421 ns |     1.520 ns |      - |      - |         - |
|    HashSetRemove |    127.28 ns |     2.496 ns |     3.157 ns |      - |      - |         - |
|     BitSetRemove |     88.75 ns |     1.783 ns |     3.028 ns |      - |      - |         - |
| BitSetRemoveMany |     72.25 ns |     0.767 ns |     0.718 ns |      - |      - |         - |
|       HashSetMap | 84,898.33 ns | 1,636.419 ns | 2,009.670 ns | 5.7373 | 0.4883 |  49,016 B |
|        BitSetMap | 31,206.59 ns |   606.957 ns |   623.300 ns | 5.7983 | 0.5493 |  48,976 B |