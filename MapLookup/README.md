# Performance of the Different Key Types for Map Lookup

I play with different ways to represent a Key and look at what the performance implicaiton is for Map lookup.

## Results

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
```

|       Method |      Mean |     Error |    StdDev |
|------------- |----------:|----------:|----------:|
|          Int |  2.455 us | 0.0329 us | 0.0292 us |
|          UoM |  2.473 us | 0.0135 us | 0.0105 us |
|       Record | 15.028 us | 0.2775 us | 0.2595 us |
| StructRecord | 17.282 us | 0.3286 us | 0.3516 us |
|           DU | 13.972 us | 0.1482 us | 0.1386 us |
|      StuctDU | 17.839 us | 0.3518 us | 0.3764 us |
|       String |  4.620 us | 0.0532 us | 0.0471 us |