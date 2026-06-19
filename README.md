# SpanBenchmarks

A .NET 9 **BenchmarkDotNet** console application that benchmarks 7 different techniques for summing elements of a list or array in C#. The goal is to measure performance (execution time) and memory allocations across varying data sizes (10, 100, 1,000, and 1,000,000 elements).

## Technologies / Libraries

- **.NET 9**
- **BenchmarkDotNet** 0.15.6

## Benchmarked Methods

| Method | Description |
|---|---|
| `Array_For` (baseline) | Simple `for` loop over an `int[]` |
| `List_Foreach` | `foreach` over `List<T>` (uses struct enumerator) |
| `List_For_Indexer` | `for` loop with indexer `list[i]` |
| `List_ToArray_Then_For` | `list.ToArray()` then `for` loop over the copy |
| `List_Linq_Sum` | `_list.Sum(x => (long)x)` |
| `List_CopyTo_PooledArray_For` | `ArrayPool<int>.Shared.Rent` + `CopyTo` + `for` |
| `List_AsReadOnlySpan` | `CollectionsMarshal.AsSpan(_list)` + `for` over the span |

## Key Features

- **Memory diagnostics** (`[MemoryDiagnoser]`) — tracks allocations per benchmark.
- **Disassembly diagnostics** (`[DisassemblyDiagnoser]`) — exports JIT assembly for analysis.
- **Parameterized sizes** — `[Params(10, 100, 1000, 1_000_000)]` to test scaling behavior.
- **Baseline comparison** — `Array_For` is the baseline; all others report ratio vs baseline.

## How to Run

```bash
dotnet run -c Release --project SpanBenchmarks
```

The results will be printed to the console and exported as reports (HTML, CSV, markdown) in the `BenchmarkDotNet.Artifacts` folder. A sample results chart is included in `Benchmark_result.png`.
