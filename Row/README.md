# Introduction to Row/ReadOnlyRow

`Row` and `ReadOnlyRow` are the workhorses of almost all simulation code that I write. They provide the speed of an array while enforcing correct Units of Measure on the indexing `int`. This removes a large number of bugs when writing high-performance code that is often using indices into arrays instead of more complex types such as Records.

This becomes especially useful if you model your domain using Struct of Arrays (SoA) instead of Array of Structs (AoS). The Units of Measure on the indexing ensures that you are not looking up data you did not intend to.


## Results

The following is a summary of the performance of `Row` versus the equivalent `Array` code. The tests are indicative of the common patterns I use in simulation code. In most cases `Row` is as fast or faster than `Array`. This additional speed is due to more aggresive inlining.

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
```

