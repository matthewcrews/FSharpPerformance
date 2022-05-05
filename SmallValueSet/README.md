# The Importance of Equality

I noticed something add when I created a collection called `SmallSet<'T>`. It was an idea I wanted to try out that ultimately didn't work out. Along the way I observed some interesting behavior related to equality in F# that I think is important to be aware of.

These are the results of my benchmarking. `SmallSet<'T>` uses the default behavior of `=`. `SmallSetComparer<'T>` uses a `EqualityComparer<'T>` for comparison. `SmallSetInt64` is that same as `SmallSet<'T>` but it has the type of `'T` hardcoded as an `int64`.

|                         Method |         Mean |        Error |       StdDev |       Median |  Gen 0 | Allocated |
|------------------------------- |-------------:|-------------:|-------------:|-------------:|-------:|----------:|
|                     HashSetAdd |     51.73 ns |     1.047 ns |     1.691 ns |     51.77 ns |      - |         - |
|                    SmallSetAdd | 37,885.11 ns | 1,360.177 ns | 4,010.513 ns | 36,939.41 ns | 6.0425 |  50,640 B |
|        SmallSetFastComparerAdd |  3,148.98 ns |   104.690 ns |   308.681 ns |  3,078.69 ns |      - |         - |
|    SmallSetEqualityComparerAdd |  1,296.15 ns |    47.839 ns |   141.055 ns |  1,238.57 ns |      - |         - |
|               SmallSetInt64Add |  1,242.65 ns |    42.019 ns |   123.893 ns |  1,206.64 ns |      - |         - |
|                  HashSetRemove |     62.81 ns |     2.262 ns |     6.671 ns |     61.11 ns |      - |         - |
|                 SmallSetRemove | 31,009.60 ns | 1,246.358 ns | 3,655.353 ns | 30,009.02 ns | 5.1270 |  43,200 B |
|     SmallSetFastComparerRemove |  2,729.23 ns |    96.869 ns |   285.621 ns |  2,662.08 ns |      - |         - |
| SmallSetEqualityComparerRemove |  1,146.31 ns |    36.934 ns |   108.900 ns |  1,101.90 ns |      - |         - |
|            SmallSetInt64Remove |    904.72 ns |    32.147 ns |    94.282 ns |    883.13 ns |      - |         - |
