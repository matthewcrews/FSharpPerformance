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


|               Method |         Mean |      Error |       StdDev |  Gen 0 |  Gen 1 | Allocated |
|--------------------- |-------------:|-----------:|-------------:|-------:|-------:|----------:|
|           HashSetAdd |    121.78 ns |   1.309 ns |     1.224 ns |      - |      - |         - |
|        HashSetRemove |    118.60 ns |   0.697 ns |     0.618 ns |      - |      - |         - |
|           HashSetMap | 79,729.70 ns | 374.712 ns |   350.506 ns | 5.7373 | 0.4883 |  49,016 B |
|            BitSetAdd |     66.24 ns |   0.180 ns |     0.160 ns |      - |      - |         - |
|         BitSetRemove |     67.86 ns |   0.212 ns |     0.177 ns |      - |      - |         - |
|            BitSetMap | 29,746.68 ns | 588.177 ns | 1,045.484 ns | 5.8289 | 0.5798 |  48,976 B |
|    InliningBitSetAdd |     49.70 ns |   1.001 ns |     1.436 ns |      - |      - |         - |
| InliningBitSetRemove |     50.01 ns |   1.027 ns |     1.716 ns |      - |      - |         - |