# Tracking Available Assignments

## Problem Description

I am working on a simulation of a Job-Shop scheduling environment. The domain is composed of Jobs, Machines, and the combination of the two which is an Assignment. An Assignment can be thought of as a two element tuple where the first element is the Job and the second element is a Machine.

I need to track which Assignments are available at any given moment. When a Job is assigned to a Machine, I need to remove the corresponding Assignments from the available pool. When a Job is finished on a machine, I need to add the now available Assignments to the pool.

This tracking of Assignments is in the hot path and currently the slowest part of the algorithm. Here I explore several different approaches.

## The Approaches

I investigate 7 different approaches

1. Default Tuple
2. Struct Tuple
3. Default Record (ref type)
4. Struct Record
5. int64<'Measure>
6. Struct wrapper of int64
7. BitArray

## Results

### Scenario 1

Machine Count: 10
Job Count : 100
Remove/Add Count: 10

|                  Method |        Mean |      Error |     StdDev |  Gen 0 |  Gen 1 | Allocated |
|------------------------ |------------:|-----------:|-----------:|-------:|-------:|----------:|
|                TupleAdd | 5,935.01 ns |  95.520 ns |  84.676 ns | 0.8850 | 0.0076 |   7,416 B |
|          StructTupleAdd | 5,930.47 ns | 114.158 ns | 156.260 ns | 1.2436 | 0.0076 |  10,424 B |
|                Int64Add | 1,253.93 ns |  24.810 ns |  48.389 ns | 0.4368 | 0.0038 |   3,656 B |
|        RefAssignmentAdd | 3,317.01 ns |  57.650 ns |  51.105 ns | 0.4349 | 0.0038 |   3,656 B |
|     StructAssignmentAdd | 3,245.86 ns |  63.452 ns | 107.746 ns | 0.8202 | 0.0076 |   6,872 B |
|    CompactAssignmentAdd | 3,314.19 ns |  65.317 ns | 122.681 ns | 0.8202 | 0.0076 |   6,872 B |
|      BitArrayTrackerAdd |    49.93 ns |   1.055 ns |   2.108 ns | 0.0258 |      - |     216 B |
|             TupleRemove | 6,033.93 ns | 119.277 ns | 146.483 ns | 0.8240 |      - |   6,904 B |
|       StructTupleRemove | 5,765.80 ns | 112.020 ns | 174.402 ns | 1.1215 | 0.0076 |   9,400 B |
|             Int64Remove | 1,100.27 ns |  21.690 ns |  28.203 ns | 0.3891 | 0.0019 |   3,256 B |
|     RefAssignmentRemove | 3,054.12 ns |  51.059 ns |  47.761 ns | 0.3891 |      - |   3,256 B |
|  StructAssignmentRemove | 2,920.25 ns |  56.931 ns |  77.928 ns | 0.7133 | 0.0038 |   5,992 B |
| CompactAssignmentRemove | 2,814.26 ns |  53.150 ns |  59.076 ns | 0.7133 | 0.0038 |   5,992 B |
|   BitArrayTrackerRemove |    48.01 ns |   1.019 ns |   2.789 ns | 0.0258 |      - |     216 B |

### Scenario 2

Machine Count: 10
Job Count : 1,000
Remove/Add Count: 10

|                  Method |        Mean |      Error |     StdDev |  Gen 0 |  Gen 1 | Allocated |
|------------------------ |------------:|-----------:|-----------:|-------:|-------:|----------:|
|                TupleAdd | 6,091.83 ns | 116.745 ns | 134.444 ns | 0.9003 | 0.0076 |      7 KB |
|          StructTupleAdd | 5,667.13 ns | 111.610 ns | 145.124 ns | 1.2436 | 0.0076 |     10 KB |
|                Int64Add | 1,207.43 ns |  22.697 ns |  24.286 ns | 0.4539 | 0.0038 |      4 KB |
|        RefAssignmentAdd | 3,320.14 ns |  63.220 ns |  77.640 ns | 0.4539 | 0.0038 |      4 KB |
|     StructAssignmentAdd | 3,082.64 ns |  57.766 ns |  56.734 ns | 0.8469 | 0.0076 |      7 KB |
|    CompactAssignmentAdd | 3,143.13 ns |  62.143 ns |  66.493 ns | 0.8469 | 0.0076 |      7 KB |
|      BitArrayTrackerAdd |    87.32 ns |   1.809 ns |   2.869 ns | 0.1606 | 0.0015 |      1 KB |
|             TupleRemove | 5,455.82 ns | 106.946 ns | 100.037 ns | 0.8011 |      - |      7 KB |
|       StructTupleRemove | 5,621.55 ns | 111.787 ns | 160.322 ns | 1.1063 | 0.0076 |      9 KB |
|             Int64Remove | 1,172.22 ns |  23.519 ns |  39.938 ns | 0.4139 | 0.0019 |      3 KB |
|     RefAssignmentRemove | 2,944.85 ns |  56.474 ns |  55.465 ns | 0.4120 |      - |      3 KB |
|  StructAssignmentRemove | 2,842.38 ns |  56.840 ns |  93.390 ns | 0.7286 | 0.0038 |      6 KB |
| CompactAssignmentRemove | 2,774.95 ns |  54.930 ns |  96.205 ns | 0.7286 | 0.0038 |      6 KB |
|   BitArrayTrackerRemove |   105.34 ns |   2.163 ns |   3.367 ns | 0.1606 | 0.0015 |      1 KB |

### Scenario 1

Machine Count: 100
Job Count : 1,000
Remove/Add Count: 10

|                  Method |       Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Allocated |
|------------------------ |-----------:|----------:|----------:|-------:|-------:|----------:|
|                TupleAdd | 6,376.8 ns | 125.31 ns | 144.31 ns | 0.9003 | 0.0076 |      7 KB |
|          StructTupleAdd | 5,614.0 ns | 111.11 ns | 148.33 ns | 1.2436 | 0.0076 |     10 KB |
|                Int64Add | 1,249.5 ns |  24.99 ns |  35.03 ns | 0.4539 | 0.0038 |      4 KB |
|        RefAssignmentAdd | 3,337.0 ns |  61.77 ns |  57.78 ns | 0.4539 | 0.0038 |      4 KB |
|     StructAssignmentAdd | 3,166.1 ns |  60.16 ns | 100.52 ns | 0.8469 | 0.0076 |      7 KB |
|    CompactAssignmentAdd | 3,200.6 ns |  62.34 ns |  69.30 ns | 0.8469 | 0.0076 |      7 KB |
|      BitArrayTrackerAdd |   556.1 ns |  10.87 ns |  15.93 ns | 1.5030 | 0.1249 |     12 KB |
|             TupleRemove | 5,878.3 ns | 114.31 ns | 112.26 ns | 0.8469 |      - |      7 KB |
|       StructTupleRemove | 5,398.2 ns | 103.93 ns | 119.68 ns | 1.1063 | 0.0076 |      9 KB |
|             Int64Remove | 1,120.5 ns |  22.47 ns |  30.76 ns | 0.4101 | 0.0019 |      3 KB |
|     RefAssignmentRemove | 2,836.0 ns |  50.76 ns |  45.00 ns | 0.4082 |      - |      3 KB |
|  StructAssignmentRemove | 2,815.4 ns |  55.42 ns |  61.60 ns | 0.7286 | 0.0038 |      6 KB |
| CompactAssignmentRemove | 2,824.4 ns |  56.23 ns |  93.95 ns | 0.7286 | 0.0038 |      6 KB |
|   BitArrayTrackerRemove |   566.3 ns |  11.32 ns |  18.29 ns | 1.5030 | 0.1249 |     12 KB |