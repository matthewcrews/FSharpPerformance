# Types for Set Remove

I test various ways to represent the same information and the performance impact it has on the `Set.Remove` method.

## Results

|       Method |     Mean |     Error |    StdDev |   Median |
|------------- |---------:|----------:|----------:|---------:|
|          Int | 1.468 us | 0.0998 us | 0.2943 us | 1.319 us |
|          UoM | 1.413 us | 0.0909 us | 0.2680 us | 1.308 us |
|       Record | 3.003 us | 0.1476 us | 0.4211 us | 2.854 us |
| StructRecord | 3.586 us | 0.2262 us | 0.6669 us | 3.189 us |
|           DU | 3.246 us | 0.2006 us | 0.5916 us | 2.932 us |
|     StructDU | 3.574 us | 0.2046 us | 0.6031 us | 3.297 us |