# Tracking Available Assignments

## Problem Description

I am working on a simulation of a Job-Shop scheduling environment. The domain is composed of Jobs, Machines, and the combination of the two which is an Assignment. An Assignment can be thought of as a two element tuple where the first element is the Job and the second element is a Machine.

I need to track which Assignments are available at any given moment. When a Job is assigned to a Machine, I need to remove the corresponding Assignments from the available pool. When a Job is finished on a machine, I need to add the now available Assignments to the pool.

This tracking of Assignments is in the hot path and currently the slowest part of the algorithm. Here I explore several different approaches.

## The Approaches

I investigate 6 different approaches

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