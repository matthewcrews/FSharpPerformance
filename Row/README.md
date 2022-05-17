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

|      Method |   Size |           Mean |       Error |        StdDev |         Median |  Gen 0 |  Gen 1 | Allocated |
|------------ |------- |---------------:|------------:|--------------:|---------------:|-------:|-------:|----------:|
|    ArraySum |      1 |      0.7539 ns |   0.0125 ns |     0.0117 ns |      0.7489 ns |      - |      - |         - |
|      RowSum |      1 |      0.5398 ns |   0.0158 ns |     0.0147 ns |      0.5343 ns |      - |      - |         - |
|  ArrayIteri |      1 |      9.4685 ns |   0.2025 ns |     0.3211 ns |      9.4790 ns | 0.0029 |      - |      24 B |
|    RowIteri |      1 |      0.7290 ns |   0.0156 ns |     0.0122 ns |      0.7314 ns |      - |      - |         - |
|    ArrayMin |      1 |      1.0243 ns |   0.0202 ns |     0.0179 ns |      1.0206 ns |      - |      - |         - |
|      RowMin |      1 |      1.1139 ns |   0.0175 ns |     0.0155 ns |      1.1180 ns |      - |      - |         - |
|  ArrayMinBy |      1 |      0.9451 ns |   0.0170 ns |     0.0159 ns |      0.9362 ns |      - |      - |         - |
|    RowMinBy |      1 |      1.1352 ns |   0.0198 ns |     0.0185 ns |      1.1267 ns |      - |      - |         - |
|   ArrayIter |      1 |      0.6652 ns |   0.0137 ns |     0.0128 ns |      0.6626 ns |      - |      - |         - |
|     RowIter |      1 |      0.4359 ns |   0.0106 ns |     0.0100 ns |      0.4333 ns |      - |      - |         - |
| ArrayIteri2 |      1 |      9.3009 ns |   0.1189 ns |     0.1112 ns |      9.3241 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |      1 |      2.7864 ns |   0.0208 ns |     0.0195 ns |      2.7788 ns |      - |      - |         - |
|    ArrayMap |      1 |      3.3578 ns |   0.0611 ns |     0.0571 ns |      3.3292 ns | 0.0038 |      - |      32 B |
|      RowMap |      1 |      8.4897 ns |   0.1342 ns |     0.1255 ns |      8.4612 ns | 0.0067 |      - |      56 B |
|   ArrayMapi |      1 |      9.2714 ns |   0.1536 ns |     0.1437 ns |      9.2240 ns | 0.0038 |      - |      32 B |
|     RowMapi |      1 |      8.0703 ns |   0.1569 ns |     0.1467 ns |      8.0437 ns | 0.0067 |      - |      56 B |
|   ArrayMap2 |      1 |      9.2081 ns |   0.1634 ns |     0.1528 ns |      9.2323 ns | 0.0038 |      - |      32 B |
|     RowMap2 |      1 |      9.8261 ns |   0.2001 ns |     0.1671 ns |      9.7490 ns | 0.0067 |      - |      56 B |
|  ArrayMapi2 |      1 |     13.9457 ns |   0.0633 ns |     0.0528 ns |     13.9512 ns | 0.0067 |      - |      56 B |
|    RowMapi2 |      1 |     10.1401 ns |   0.1131 ns |     0.1058 ns |     10.1714 ns | 0.0067 |      - |      56 B |
|    ArraySum |     10 |      4.5204 ns |   0.0675 ns |     0.0631 ns |      4.5194 ns |      - |      - |         - |
|      RowSum |     10 |      3.5220 ns |   0.0541 ns |     0.0506 ns |      3.5116 ns |      - |      - |         - |
|  ArrayIteri |     10 |     24.0403 ns |   0.3392 ns |     0.3007 ns |     24.0153 ns | 0.0029 |      - |      24 B |
|    RowIteri |     10 |      5.3433 ns |   0.0998 ns |     0.0934 ns |      5.3300 ns |      - |      - |         - |
|    ArrayMin |     10 |      5.8394 ns |   0.1072 ns |     0.1002 ns |      5.8438 ns |      - |      - |         - |
|      RowMin |     10 |      5.9542 ns |   0.1413 ns |     0.1451 ns |      5.8920 ns |      - |      - |         - |
|  ArrayMinBy |     10 |      5.9911 ns |   0.0895 ns |     0.0837 ns |      5.9916 ns |      - |      - |         - |
|    RowMinBy |     10 |      6.1303 ns |   0.0654 ns |     0.0611 ns |      6.1414 ns |      - |      - |         - |
|   ArrayIter |     10 |      3.5936 ns |   0.0996 ns |     0.1147 ns |      3.5416 ns |      - |      - |         - |
|     RowIter |     10 |      3.6140 ns |   0.1012 ns |     0.1280 ns |      3.6117 ns |      - |      - |         - |
| ArrayIteri2 |     10 |     25.0836 ns |   0.5025 ns |     0.4700 ns |     24.9291 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |     10 |      8.4160 ns |   0.1666 ns |     0.1391 ns |      8.3906 ns |      - |      - |         - |
|    ArrayMap |     10 |      9.3757 ns |   0.2497 ns |     0.7323 ns |      9.4012 ns | 0.0076 |      - |      64 B |
|      RowMap |     10 |     14.5926 ns |   0.4611 ns |     1.3594 ns |     14.5101 ns | 0.0105 |      - |      88 B |
|   ArrayMapi |     10 |     26.3767 ns |   0.5837 ns |     1.6367 ns |     26.1171 ns | 0.0076 |      - |      64 B |
|     RowMapi |     10 |     15.2028 ns |   0.3804 ns |     1.1218 ns |     15.4308 ns | 0.0105 |      - |      88 B |
|   ArrayMap2 |     10 |     23.7570 ns |   0.5607 ns |     1.6532 ns |     23.6968 ns | 0.0076 |      - |      64 B |
|     RowMap2 |     10 |     16.8298 ns |   0.4763 ns |     1.4044 ns |     16.9738 ns | 0.0105 |      - |      88 B |
|  ArrayMapi2 |     10 |     46.1994 ns |   1.0360 ns |     3.0548 ns |     45.4873 ns | 0.0153 |      - |     128 B |
|    RowMapi2 |     10 |     17.1765 ns |   0.4005 ns |     0.7812 ns |     16.9346 ns | 0.0153 |      - |     128 B |
|    ArraySum |    100 |     45.1369 ns |   0.2672 ns |     0.2369 ns |     45.0492 ns |      - |      - |         - |
|      RowSum |    100 |     40.3771 ns |   0.5182 ns |     0.4847 ns |     40.3034 ns |      - |      - |         - |
|  ArrayIteri |    100 |    176.9081 ns |   0.7817 ns |     0.6528 ns |    176.8135 ns | 0.0029 |      - |      24 B |
|    RowIteri |    100 |     55.3395 ns |   1.0215 ns |     0.9056 ns |     55.5164 ns |      - |      - |         - |
|    ArrayMin |    100 |     54.0848 ns |   0.8612 ns |     0.7634 ns |     54.1868 ns |      - |      - |         - |
|      RowMin |    100 |     55.1601 ns |   1.0578 ns |     1.0863 ns |     54.8568 ns |      - |      - |         - |
|  ArrayMinBy |    100 |     78.5190 ns |   1.5720 ns |     2.4934 ns |     77.8904 ns |      - |      - |         - |
|    RowMinBy |    100 |     55.7768 ns |   1.1364 ns |     2.2164 ns |     55.4788 ns |      - |      - |         - |
|   ArrayIter |    100 |     43.9904 ns |   0.9059 ns |     1.0784 ns |     44.0506 ns |      - |      - |         - |
|     RowIter |    100 |     37.6733 ns |   0.7696 ns |     0.9733 ns |     37.5242 ns |      - |      - |         - |
| ArrayIteri2 |    100 |    186.3054 ns |   3.5771 ns |     4.2582 ns |    185.5995 ns | 0.0029 |      - |      24 B |
|   RowIteri2 |    100 |     53.4409 ns |   1.0819 ns |     1.0625 ns |     53.4414 ns |      - |      - |         - |
|    ArrayMap |    100 |     83.3210 ns |   1.9235 ns |     5.6715 ns |     83.0444 ns | 0.0507 |      - |     424 B |
|      RowMap |    100 |     85.1095 ns |   1.7908 ns |     5.1954 ns |     83.5175 ns | 0.0535 |      - |     448 B |
|   ArrayMapi |    100 |    187.8419 ns |   3.8086 ns |     8.4395 ns |    188.3401 ns | 0.0505 |      - |     424 B |
|     RowMapi |    100 |     79.3454 ns |   1.1887 ns |     0.9281 ns |     79.2414 ns | 0.0535 |      - |     448 B |
|   ArrayMap2 |    100 |    164.4180 ns |   3.3349 ns |     7.5275 ns |    165.4254 ns | 0.0505 |      - |     424 B |
|     RowMap2 |    100 |     86.1258 ns |   1.1340 ns |     1.0053 ns |     86.1918 ns | 0.0535 |      - |     448 B |
|  ArrayMapi2 |    100 |    350.5879 ns |   7.0668 ns |    20.1620 ns |    349.9436 ns | 0.1011 |      - |     848 B |
|    RowMapi2 |    100 |     98.3611 ns |   2.5065 ns |     7.3903 ns |     98.3672 ns | 0.1013 | 0.0002 |     848 B |
|    ArraySum |  1_000 |    363.1056 ns |   5.3004 ns |     4.9580 ns |    361.3814 ns |      - |      - |         - |
|      RowSum |  1_000 |    318.6828 ns |   6.0691 ns |     5.6771 ns |    317.5072 ns |      - |      - |         - |
|  ArrayIteri |  1_000 |  1,757.6617 ns |  28.9888 ns |    27.1161 ns |  1,758.8736 ns | 0.0019 |      - |      24 B |
|    RowIteri |  1_000 |    470.0566 ns |   2.5718 ns |     2.4056 ns |    469.3994 ns |      - |      - |         - |
|    ArrayMin |  1_000 |    490.7733 ns |   9.7377 ns |     9.1087 ns |    490.5755 ns |      - |      - |         - |
|      RowMin |  1_000 |    487.5176 ns |   5.2698 ns |     4.4005 ns |    488.3946 ns |      - |      - |         - |
|  ArrayMinBy |  1_000 |    479.0744 ns |   5.3687 ns |     4.1915 ns |    479.7377 ns |      - |      - |         - |
|    RowMinBy |  1_000 |    482.4167 ns |   7.4449 ns |     6.5997 ns |    479.9889 ns |      - |      - |         - |
|   ArrayIter |  1_000 |    376.3208 ns |   5.3549 ns |     4.7470 ns |    374.5399 ns |      - |      - |         - |
|     RowIter |  1_000 |    343.1699 ns |   3.2265 ns |     2.8602 ns |    342.3287 ns |      - |      - |         - |
| ArrayIteri2 |  1_000 |  1,727.4469 ns |  32.7740 ns |    32.1885 ns |  1,724.4954 ns | 0.0019 |      - |      24 B |
|   RowIteri2 |  1_000 |    474.8059 ns |   3.8658 ns |     3.6161 ns |    473.6705 ns |      - |      - |         - |
|    ArrayMap |  1_000 |    652.5583 ns |   7.4305 ns |     6.2048 ns |    649.5756 ns | 0.4807 | 0.0067 |   4,024 B |
|      RowMap |  1_000 |    669.6666 ns |   3.1920 ns |     2.6654 ns |    668.6288 ns | 0.4826 | 0.0134 |   4,048 B |
|   ArrayMapi |  1_000 |  1,557.6238 ns |  16.3428 ns |    15.2870 ns |  1,550.5024 ns | 0.4807 | 0.0057 |   4,024 B |
|     RowMapi |  1_000 |    669.6844 ns |   3.6176 ns |     3.0209 ns |    669.2671 ns | 0.4826 | 0.0134 |   4,048 B |
|   ArrayMap2 |  1_000 |  1,314.7725 ns |  10.5150 ns |     8.7805 ns |  1,311.4172 ns | 0.4807 | 0.0057 |   4,024 B |
|     RowMap2 |  1_000 |    722.9521 ns |  10.4864 ns |    15.6955 ns |    719.1503 ns | 0.4826 | 0.0134 |   4,048 B |
|  ArrayMapi2 |  1_000 |  3,191.1585 ns |  63.2212 ns |   102.0904 ns |  3,140.5931 ns | 0.9613 | 0.0267 |   8,048 B |
|    RowMapi2 |  1_000 |    739.0508 ns |   8.2695 ns |     7.7353 ns |    738.6287 ns | 0.9613 | 0.0277 |   8,048 B |
|    ArraySum | 10_000 |  3,552.0375 ns |  55.1283 ns |    46.0346 ns |  3,528.1174 ns |      - |      - |         - |
|      RowSum | 10_000 |  3,056.7089 ns |  40.9916 ns |    36.3380 ns |  3,044.4902 ns |      - |      - |         - |
|  ArrayIteri | 10_000 | 17,582.5285 ns | 310.6962 ns |   290.6254 ns | 17,524.4537 ns |      - |      - |      24 B |
|    RowIteri | 10_000 |  4,868.9691 ns |  95.3326 ns |   123.9593 ns |  4,834.5238 ns |      - |      - |         - |
|    ArrayMin | 10_000 |  4,741.2123 ns |  85.0078 ns |    97.8951 ns |  4,732.5649 ns |      - |      - |         - |
|      RowMin | 10_000 |  4,716.9010 ns |  75.5171 ns |    70.6388 ns |  4,701.7502 ns |      - |      - |         - |
|  ArrayMinBy | 10_000 |  4,696.3208 ns |  45.7635 ns |    40.5682 ns |  4,688.1737 ns |      - |      - |         - |
|    RowMinBy | 10_000 |  4,787.7750 ns |  86.2142 ns |   118.0110 ns |  4,738.2748 ns |      - |      - |         - |
|   ArrayIter | 10_000 |  3,719.3468 ns |  67.4325 ns |    63.0764 ns |  3,686.1008 ns |      - |      - |         - |
|     RowIter | 10_000 |  3,541.1503 ns |  68.4974 ns |    64.0725 ns |  3,519.9913 ns |      - |      - |         - |
| ArrayIteri2 | 10_000 | 17,354.3219 ns | 189.3605 ns |   167.8631 ns | 17,376.4297 ns |      - |      - |      24 B |
|   RowIteri2 | 10_000 |  4,861.1040 ns |  96.1712 ns |   134.8185 ns |  4,804.4395 ns |      - |      - |         - |
|    ArrayMap | 10_000 |  6,660.4332 ns | 180.0017 ns |   530.7391 ns |  6,358.7162 ns | 4.7607 | 0.5951 |  40,024 B |
|      RowMap | 10_000 |  6,033.0986 ns | 120.6219 ns |   277.1489 ns |  5,895.4681 ns | 4.7607 | 0.9460 |  40,048 B |
|   ArrayMapi | 10_000 | 15,075.9990 ns |  58.5261 ns |    48.8720 ns | 15,055.5359 ns | 4.7607 | 0.5951 |  40,024 B |
|     RowMapi | 10_000 |  5,790.0675 ns |  31.9602 ns |    29.8956 ns |  5,781.8298 ns | 4.7607 | 0.9460 |  40,048 B |
|   ArrayMap2 | 10_000 | 13,934.0663 ns | 340.1437 ns | 1,002.9213 ns | 13,712.8555 ns | 4.7607 | 0.5951 |  40,024 B |
|     RowMap2 | 10_000 |  6,619.8994 ns | 131.5810 ns |   259.7281 ns |  6,578.3779 ns | 4.7607 | 0.9460 |  40,048 B |
|  ArrayMapi2 | 10_000 | 30,782.1551 ns | 766.0387 ns | 2,258.6823 ns | 30,193.4830 ns | 9.5215 | 1.8921 |  80,048 B |
|    RowMapi2 | 10_000 |  7,164.4130 ns | 142.6388 ns |   126.4456 ns |  7,128.9059 ns | 9.5215 | 1.8997 |  80,048 B |