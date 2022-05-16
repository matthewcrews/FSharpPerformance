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

|      Method |   Size |           Mean |       Error |      StdDev |         Median |  Gen 0 |  Gen 1 | Allocated |
|------------ |------- |---------------:|------------:|------------:|---------------:|-------:|-------:|----------:|
|    ArraySum |      1 |      0.7337 ns |   0.0090 ns |   0.0079 ns |      0.7350 ns |      - |      - |         - |
|      RowSum |      1 |      0.5567 ns |   0.0052 ns |   0.0046 ns |      0.5576 ns |      - |      - |         - |
|  ArrayIteri |      1 |     10.6524 ns |   0.1416 ns |   0.1183 ns |     10.6267 ns | 0.0029 |      - |      24 B |
|    RowIteri |      1 |      0.7600 ns |   0.0201 ns |   0.0188 ns |      0.7548 ns |      - |      - |         - |
|    ArrayMin |      1 |      1.0472 ns |   0.0141 ns |   0.0132 ns |      1.0445 ns |      - |      - |         - |
|      RowMin |      1 |      1.1796 ns |   0.0513 ns |   0.0455 ns |      1.1609 ns |      - |      - |         - |
|  ArrayMinBy |      1 |      0.9098 ns |   0.0067 ns |   0.0056 ns |      0.9087 ns |      - |      - |         - |
|    RowMinBy |      1 |      1.1220 ns |   0.0264 ns |   0.0247 ns |      1.1181 ns |      - |      - |         - |
|   ArrayIter |      1 |      0.6927 ns |   0.0220 ns |   0.0206 ns |      0.6948 ns |      - |      - |         - |
|     RowIter |      1 |      0.4728 ns |   0.0220 ns |   0.0195 ns |      0.4744 ns |      - |      - |         - |
| ArrayIteri2 |      1 |      9.7354 ns |   0.2213 ns |   0.4211 ns |      9.7347 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |      1 |      2.8021 ns |   0.0172 ns |   0.0152 ns |      2.7998 ns |      - |      - |         - |
|    ArrayMap |      1 |      3.4451 ns |   0.0501 ns |   0.0468 ns |      3.4419 ns | 0.0038 |      - |      32 B |
|      RowMap |      1 |      9.8287 ns |   0.2445 ns |   0.2616 ns |      9.7483 ns | 0.0067 |      - |      56 B |
|   ArrayMapi |      1 |      9.1417 ns |   0.1551 ns |   0.1451 ns |      9.1643 ns | 0.0038 |      - |      32 B |
|     RowMapi |      1 |      9.5116 ns |   0.1596 ns |   0.1246 ns |      9.4783 ns | 0.0067 |      - |      56 B |
|   ArrayMap2 |      1 |      9.8483 ns |   0.2538 ns |   0.4510 ns |      9.6893 ns | 0.0038 |      - |      32 B |
|     RowMap2 |      1 |     12.2391 ns |   0.2988 ns |   0.6302 ns |     12.1187 ns | 0.0067 |      - |      56 B |
|  ArrayMapi2 |      1 |     15.0281 ns |   0.3570 ns |   0.7836 ns |     14.7443 ns | 0.0067 |      - |      56 B |
|    RowMapi2 |      1 |     12.9335 ns |   0.3166 ns |   0.7885 ns |     12.7910 ns | 0.0067 |      - |      56 B |
|    ArraySum |     10 |      4.4291 ns |   0.0525 ns |   0.0438 ns |      4.4233 ns |      - |      - |         - |
|      RowSum |     10 |      3.3768 ns |   0.0353 ns |   0.0330 ns |      3.3716 ns |      - |      - |         - |
|  ArrayIteri |     10 |     25.8154 ns |   0.1629 ns |   0.1524 ns |     25.7922 ns | 0.0029 |      - |      24 B |
|    RowIteri |     10 |      5.1351 ns |   0.0245 ns |   0.0217 ns |      5.1335 ns |      - |      - |         - |
|    ArrayMin |     10 |      5.7461 ns |   0.1324 ns |   0.1238 ns |      5.7453 ns |      - |      - |         - |
|      RowMin |     10 |      5.8803 ns |   0.0825 ns |   0.0731 ns |      5.8736 ns |      - |      - |         - |
|  ArrayMinBy |     10 |      7.9719 ns |   0.1803 ns |   0.2146 ns |      7.9668 ns |      - |      - |         - |
|    RowMinBy |     10 |      5.6172 ns |   0.0941 ns |   0.0834 ns |      5.6174 ns |      - |      - |         - |
|   ArrayIter |     10 |      3.6993 ns |   0.0598 ns |   0.0559 ns |      3.6756 ns |      - |      - |         - |
|     RowIter |     10 |      3.5602 ns |   0.0966 ns |   0.0949 ns |      3.5654 ns |      - |      - |         - |
| ArrayIteri2 |     10 |     23.9365 ns |   0.4968 ns |   0.4647 ns |     23.7082 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |     10 |      8.3235 ns |   0.0332 ns |   0.0277 ns |      8.3160 ns |      - |      - |         - |
|    ArrayMap |     10 |      8.5530 ns |   0.1643 ns |   0.1537 ns |      8.5187 ns | 0.0076 |      - |      64 B |
|      RowMap |     10 |     14.1742 ns |   0.1359 ns |   0.1204 ns |     14.1985 ns | 0.0105 |      - |      88 B |
|   ArrayMapi |     10 |     23.6370 ns |   0.1270 ns |   0.1126 ns |     23.6330 ns | 0.0076 |      - |      64 B |
|     RowMapi |     10 |     14.7726 ns |   0.3506 ns |   0.4306 ns |     14.6167 ns | 0.0105 |      - |      88 B |
|   ArrayMap2 |     10 |     23.9962 ns |   0.3563 ns |   0.2782 ns |     23.9651 ns | 0.0076 |      - |      64 B |
|     RowMap2 |     10 |     16.9227 ns |   0.3940 ns |   0.6474 ns |     16.8117 ns | 0.0105 |      - |      88 B |
|  ArrayMapi2 |     10 |     41.1621 ns |   0.5045 ns |   0.4472 ns |     40.9420 ns | 0.0153 |      - |     128 B |
|    RowMapi2 |     10 |     21.5306 ns |   0.3908 ns |   0.3655 ns |     21.4107 ns | 0.0153 |      - |     128 B |
|    ArraySum |    100 |     42.1422 ns |   0.8493 ns |   0.7945 ns |     41.7788 ns |      - |      - |         - |
|      RowSum |    100 |     39.6770 ns |   0.2405 ns |   0.2008 ns |     39.5816 ns |      - |      - |         - |
|  ArrayIteri |    100 |    180.1558 ns |   1.5448 ns |   1.4450 ns |    179.7635 ns | 0.0029 |      - |      24 B |
|    RowIteri |    100 |     54.6550 ns |   0.6996 ns |   0.6544 ns |     54.5263 ns |      - |      - |         - |
|    ArrayMin |    100 |     55.0159 ns |   0.9695 ns |   0.9069 ns |     54.7585 ns |      - |      - |         - |
|      RowMin |    100 |     54.4124 ns |   1.0774 ns |   1.1975 ns |     53.6423 ns |      - |      - |         - |
|  ArrayMinBy |    100 |     76.7359 ns |   0.9762 ns |   0.9131 ns |     76.5431 ns |      - |      - |         - |
|    RowMinBy |    100 |     53.6006 ns |   0.9070 ns |   0.8040 ns |     53.2367 ns |      - |      - |         - |
|   ArrayIter |    100 |     42.8760 ns |   0.5299 ns |   0.4956 ns |     43.0255 ns |      - |      - |         - |
|     RowIter |    100 |     36.2858 ns |   0.6673 ns |   0.6242 ns |     36.3456 ns |      - |      - |         - |
| ArrayIteri2 |    100 |    178.7305 ns |   0.9997 ns |   0.9351 ns |    178.5699 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |    100 |     55.0547 ns |   0.7728 ns |   0.6851 ns |     55.0081 ns |      - |      - |         - |
|    ArrayMap |    100 |     79.5528 ns |   1.4785 ns |   1.9225 ns |     79.4148 ns | 0.0507 |      - |     424 B |
|      RowMap |    100 |     87.8338 ns |   1.7423 ns |   2.8134 ns |     87.6850 ns | 0.0535 |      - |     448 B |
|   ArrayMapi |    100 |    157.9515 ns |   2.9908 ns |   2.3350 ns |    157.9042 ns | 0.0505 |      - |     424 B |
|     RowMapi |    100 |     89.0194 ns |   1.7979 ns |   2.9541 ns |     89.2426 ns | 0.0535 |      - |     448 B |
|   ArrayMap2 |    100 |    154.7195 ns |   3.0479 ns |   3.6284 ns |    155.7298 ns | 0.0505 |      - |     424 B |
|     RowMap2 |    100 |     94.5293 ns |   1.9302 ns |   2.6421 ns |     94.9994 ns | 0.0535 |      - |     448 B |
|  ArrayMapi2 |    100 |    322.2453 ns |   6.2730 ns |   7.7038 ns |    319.4715 ns | 0.1011 |      - |     848 B |
|    RowMapi2 |    100 |    111.4696 ns |   0.6204 ns |   0.4844 ns |    111.4274 ns | 0.1013 | 0.0002 |     848 B |
|    ArraySum |  1_000 |    366.7265 ns |   7.3491 ns |  10.7722 ns |    372.4718 ns |      - |      - |         - |
|      RowSum |  1_000 |    314.8048 ns |   6.1999 ns |   5.7994 ns |    312.4713 ns |      - |      - |         - |
|    RowIteri |  1_000 |    476.5307 ns |   2.9569 ns |   2.7659 ns |    476.6138 ns |      - |      - |         - |
|  ArrayIteri |  1_000 |  1,694.0197 ns |  11.3207 ns |  10.0355 ns |  1,698.9795 ns | 0.0019 |      - |      24 B |
|    ArrayMin |  1_000 |    494.4779 ns |   9.7826 ns |  10.8734 ns |    489.9701 ns |      - |      - |         - |
|      RowMin |  1_000 |    496.6225 ns |   9.8469 ns |  14.1222 ns |    490.4335 ns |      - |      - |         - |
|  ArrayMinBy |  1_000 |    740.8749 ns |  14.7047 ns |  17.5049 ns |    731.8222 ns |      - |      - |         - |
|    RowMinBy |  1_000 |    488.7568 ns |   5.0474 ns |   4.2148 ns |    488.3903 ns |      - |      - |         - |
|   ArrayIter |  1_000 |    374.4144 ns |   7.4334 ns |  12.6224 ns |    375.1865 ns |      - |      - |         - |
|     RowIter |  1_000 |    349.1612 ns |   3.2731 ns |   2.7332 ns |    348.2333 ns |      - |      - |         - |
| ArrayIteri2 |  1_000 |  1,694.7073 ns |   7.0129 ns |   5.4752 ns |  1,694.7907 ns | 0.0019 |      - |      24 B |
|   RowIteri2 |  1_000 |    439.0684 ns |   2.2557 ns |   1.7611 ns |    439.5386 ns |      - |      - |         - |
|    ArrayMap |  1_000 |    679.8552 ns |  10.5750 ns |   8.2563 ns |    680.3069 ns | 0.4807 | 0.0067 |   4,024 B |
|      RowMap |  1_000 |    711.0172 ns |  14.2011 ns |  13.2837 ns |    710.4128 ns | 0.4835 | 0.0067 |   4,048 B |
|   ArrayMapi |  1_000 |  1,674.7382 ns |  31.7917 ns |  32.6477 ns |  1,663.3228 ns | 0.4807 | 0.0057 |   4,024 B |
|     RowMapi |  1_000 |    703.9318 ns |  10.0713 ns |   8.9280 ns |    704.6860 ns | 0.4835 | 0.0067 |   4,048 B |
|   ArrayMap2 |  1_000 |  1,679.8827 ns |  32.6326 ns |  42.4316 ns |  1,666.8983 ns | 0.4807 | 0.0057 |   4,024 B |
|     RowMap2 |  1_000 |    748.7903 ns |  12.6704 ns |   9.8922 ns |    750.4359 ns | 0.4835 | 0.0067 |   4,048 B |
|  ArrayMapi2 |  1_000 |  3,064.5549 ns |  51.5911 ns |  55.2019 ns |  3,057.2323 ns | 0.9613 | 0.0267 |   8,048 B |
|    RowMapi2 |  1_000 |    955.8425 ns |  18.7555 ns |  32.3523 ns |    949.8233 ns | 0.9613 | 0.0267 |   8,048 B |
|    ArraySum | 10_000 |  3,740.3254 ns |  71.8347 ns |  93.4054 ns |  3,738.8346 ns |      - |      - |         - |
|      RowSum | 10_000 |  3,168.9844 ns |  62.5995 ns |  72.0897 ns |  3,129.5019 ns |      - |      - |         - |
|  ArrayIteri | 10_000 | 17,725.9452 ns | 343.4111 ns | 321.2270 ns | 17,727.1729 ns |      - |      - |      24 B |
|    RowIteri | 10_000 |  4,838.0514 ns |  94.6614 ns |  97.2103 ns |  4,822.8455 ns |      - |      - |         - |
|    ArrayMin | 10_000 |  4,799.6937 ns |  92.9158 ns |  86.9135 ns |  4,762.0514 ns |      - |      - |         - |
|      RowMin | 10_000 |  4,756.8019 ns |  49.7539 ns |  38.8446 ns |  4,751.0784 ns |      - |      - |         - |
|  ArrayMinBy | 10_000 |  7,124.8114 ns |  47.4806 ns |  39.6485 ns |  7,131.6269 ns |      - |      - |         - |
|    RowMinBy | 10_000 |  4,857.1465 ns |  96.4446 ns | 118.4426 ns |  4,821.6564 ns |      - |      - |         - |
|   ArrayIter | 10_000 |  3,689.7258 ns |  72.3712 ns |  94.1030 ns |  3,652.0288 ns |      - |      - |         - |
|     RowIter | 10_000 |  3,500.7369 ns |  18.3595 ns |  16.2752 ns |  3,501.4225 ns |      - |      - |         - |
| ArrayIteri2 | 10_000 | 17,300.5434 ns | 312.1731 ns | 292.0069 ns | 17,317.5446 ns |      - |      - |      24 B |
|   RowIteri2 | 10_000 |  4,545.2999 ns |  85.6766 ns |  95.2294 ns |  4,509.6367 ns |      - |      - |         - |
|    ArrayMap | 10_000 |  6,773.2684 ns |  96.3078 ns |  85.3743 ns |  6,765.7288 ns | 4.7607 | 0.5951 |  40,024 B |
|      RowMap | 10_000 |  6,700.9300 ns |  96.7886 ns |  85.8006 ns |  6,737.2364 ns | 4.7607 | 0.5951 |  40,048 B |
|   ArrayMapi | 10_000 | 16,526.9814 ns | 327.8199 ns | 437.6301 ns | 16,487.5610 ns | 4.7607 | 0.5798 |  40,024 B |
|     RowMapi | 10_000 |  6,765.8714 ns |  54.4177 ns |  45.4412 ns |  6,780.4901 ns | 4.7607 | 0.5951 |  40,048 B |
|   ArrayMap2 | 10_000 | 13,904.8263 ns | 276.5750 ns | 339.6589 ns | 13,755.2040 ns | 4.7607 | 0.5951 |  40,024 B |
|     RowMap2 | 10_000 |  7,375.6657 ns | 144.8050 ns | 172.3800 ns |  7,288.4499 ns | 4.7607 | 0.5951 |  40,048 B |
|  ArrayMapi2 | 10_000 | 30,088.3897 ns | 506.7352 ns | 423.1468 ns | 30,114.1235 ns | 9.5215 | 1.8921 |  80,048 B |
|    RowMapi2 | 10_000 |  9,216.6784 ns | 177.1227 ns | 248.3013 ns |  9,212.0453 ns | 9.5215 | 1.8921 |  80,048 B |