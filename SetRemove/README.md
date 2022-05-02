# Types for Set Remove

I test various ways to represent the same information and the performance impact it has on the `Set.Remove` method.

## Results

|             Method |     Mean |     Error |    StdDev |  Gen 0 | Allocated |
|------------------- |---------:|----------:|----------:|-------:|----------:|
|                Int | 1.119 us | 0.0223 us | 0.0548 us | 0.3510 |      3 KB |
|                UoM | 1.103 us | 0.0218 us | 0.0260 us | 0.3510 |      3 KB |
|             Record | 2.562 us | 0.0512 us | 0.0949 us | 0.4044 |      3 KB |
|       StructRecord | 2.560 us | 0.0470 us | 0.0811 us | 0.7248 |      6 KB |
| CustomStructRecord | 3.524 us | 0.0667 us | 0.0844 us | 0.8965 |      7 KB |
|                 DU | 2.480 us | 0.0412 us | 0.0366 us | 0.4044 |      3 KB |
|           StructDU | 2.603 us | 0.0517 us | 0.0432 us | 0.7248 |      6 KB |
|   ImmutableHashSet | 1.991 us | 0.0356 us | 0.0487 us | 0.3624 |      3 KB |