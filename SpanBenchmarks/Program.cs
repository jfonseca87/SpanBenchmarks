using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Buffers;
using System.Runtime.InteropServices;

namespace SpanBenchmarks;

public static class Program
{
    public static void Main(string[] args)
        => BenchmarkRunner.Run<ListSumBenchmarks>();
}

/// <summary>
/// Compara distintas formas de sumar elementos de una lista.
/// Importante: durante cada benchmark NO se cambia el tamaño de la lista,
/// para que el span sobre List sea seguro.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(exportCombinedDisassemblyReport: false)]
public class ListSumBenchmarks
{
    private int[] _array = [];
    private List<int> _list = [];

    [Params(10, 100, 1000, 1_000_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _array = new int[N];
        for (int i = 0; i < N; i++) _array[i] = rnd.Next(0, 100);

        _list = new List<int>(capacity: N);
        _list.AddRange(_array);
    }

    // for sobre array (lo más simple/rápido de referencia) ===
    [Benchmark(Baseline = true)]
    public long Array_For()
    {
        long sum = 0;
        var arr = _array;
        for (int i = 0; i < arr.Length; i++)
            sum += arr[i];
        return sum;
    }

    // foreach sobre List<T> (cómodo, pero con el enumerador de List)
    [Benchmark]
    public long List_Foreach()
    {
        long sum = 0;
        foreach (var x in _list)
            sum += x;
        return sum;
    }

    // for indexado sobre List<T> (evita el enumerador, pero sigue habiendo bounds checks)
    [Benchmark]
    public long List_For_Indexer()
    {
        long sum = 0;
        var list = _list;
        for (int i = 0; i < list.Count; i++)
            sum += list[i];
        return sum;
    }

    // Variante con ToArray() como “camino seguro” si no puedes garantizar estabilidad
    // de la List (asigna memoria, pero el bucle resultante es tan rápido como Array_For).
    [Benchmark]
    public long List_ToArray_Then_For()
    {
        var arr = _list.ToArray();
        long sum = 0;
        for (int i = 0; i < arr.Length; i++)
            sum += arr[i];
        return sum;
    }

    // LINQ como referencia de estilo declarativo
    [Benchmark]
    public long List_Linq_Sum()
        => _list.Sum(x => (long)x);

    // Extra: usar ArrayPool para tener un “array” temporal sin nueva asignación
    // (demostrativo; aquí no aporta mucho, pero útil cuando transformas la lista primero).
    [Benchmark]
    public long List_CopyTo_PooledArray_For()
    {
        var pool = ArrayPool<int>.Shared;
        var rented = pool.Rent(_list.Count);
        try
        {
            _list.CopyTo(rented, 0);
            long sum = 0;
            for (int i = 0; i < _list.Count; i++)
                sum += rented[i];
            return sum;
        }
        finally
        {
            pool.Return(rented);
        }
    }

    // ReadOnlySpan<int> sobre el buffer interno de List<T>.
    [Benchmark]
    public long List_AsReadOnlySpan()
    {
        long sum = 0;
        ReadOnlySpan<int> span = CollectionsMarshal.AsSpan(_list);
        for (int i = 0; i < span.Length; i++)
            sum += span[i];
        return sum;
    }
}
