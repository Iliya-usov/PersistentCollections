using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PersistentCollections.Map;

namespace PersistentCollections;

public class Program
{
    public static void Main(string[] args)
    {
        Warmup();


        var c = 0;
        while (true)
        {
            {
                Test();
            }
        }
    }

        
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Test()
    {
        DoGC();
        var initialMemory = GC.GetTotalMemory(true);
        
        Interlocked.MemoryBarrier();
        
        
        var (persistentMap, immutableMap,map) = PersistentHashMapBuilder(10_000_000, false);
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 3; i++)
        {
            DoGC();
        }
        Console.WriteLine($"Total memory: {(GC.GetTotalMemory(true) - initialMemory) / (1024.0 * 1024.0):F2} MB");

        Console.WriteLine($"GC took: {stopwatch.ElapsedMilliseconds}ms");
        DoTestEnumeration(persistentMap, immutableMap);
        GC.KeepAlive(map);

        void DoGC()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCApproach();
            GC.WaitForFullGCComplete();
        }
    }

    private static void Warmup()
    {
        for (int i = 0; i < 1000; i++)
        {
            var (persistentMapBuilder, immutableMapBuilder, _) = PersistentHashMapBuilder(100, true);
            persistentMapBuilder.ToList();
            immutableMapBuilder.ToList();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (PersistentHashMap<int, int> persistentMapBuilder, ImmutableDictionary<int, int> immutableMapBuilder, IDictionary<int, int>) PersistentHashMapBuilder(int n, bool warmup)
    {
        Print("start", warmup);
        var random = new Random(42);
        var array = Enumerable.Range(0, n).Select(_ => random.Next()).ToArray();
        Print("array", warmup);
            
        var persistentMapBuilder = PersistentHashMap<int, int>.Empty.ToBuilder();
        var immutableMapBuilder = ImmutableDictionary<int, int>.Empty.ToBuilder();

        // var map = new Dictionary<int, int>();
        var map = new Dictionary<int, int>();
            
        Print("build started", warmup);
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            // persistentMapBuilder[array[i]] = array[i];
            immutableMapBuilder[array[i]] = array[i];
                
            // map[array[i]] = array[i];
        }

        var r = (persistentMapBuilder.Build(), immutableMapBuilder.ToImmutable(), map);
        Print($"build finished: {stopwatch.ElapsedMilliseconds} ms", warmup);

        return r;
    }

    private static void Print(string message, bool warmpup)
    {
        if (warmpup) return;
        Console.WriteLine(message);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DoTestEnumeration(PersistentHashMap<int, int> persistentMap, ImmutableDictionary<int, int> immutableMap)
    {
        GC.KeepAlive(persistentMap);
        GC.KeepAlive(immutableMap);
        return;
            
        {
            var stopwatch = Stopwatch.StartNew();
            TestPersistentMapEnumeration(persistentMap);
                
            stopwatch.Stop();
            Console.WriteLine("Persistent Map Enumeration " + stopwatch.ElapsedMilliseconds + " ms");
        } 
            
        {
            var stopwatch = Stopwatch.StartNew();
            TestImmutableMapEnumeartion(immutableMap);
            stopwatch.Stop();
            Console.WriteLine("Immutable Dictionary Enumeration " + stopwatch.ElapsedMilliseconds + " ms");
        }
            
                        
        {
            var stopwatch = Stopwatch.StartNew();
            TestPersistentMapEnumeration(persistentMap);
                
            stopwatch.Stop();
            Console.WriteLine("Persistent Map Enumeration " + stopwatch.ElapsedMilliseconds + " ms");
        } 
            
        {
            var stopwatch = Stopwatch.StartNew();
            TestImmutableMapEnumeartion(immutableMap);
            stopwatch.Stop();
            Console.WriteLine("Immutable Dictionary Enumeration " + stopwatch.ElapsedMilliseconds + " ms");
        }

        Console.WriteLine();
    }

    private static void UltraGc()
    {
        var refs = Enumerable.Range(0, 20).Select(x => Alloc()).ToList();

        do
        {
            UltraCollect();
            var count = 0;
            foreach (var weakReference in refs)
            {
                if (!weakReference.TryGetTarget(out var target))
                    count++;
            }
                
            if (count == refs.Count)
                return;
                
        } while (true);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<object> Alloc()
    {
        var b = new byte[50 * 1024];
        do
        {
            UltraCollect();
        } while (GC.GetGeneration(b) != GC.MaxGeneration);
                
        GC.KeepAlive(b);
        return new WeakReference<object>(b);
    }

    private static void UltraCollect()
    {
        GC.GetTotalMemory(true);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.GetTotalMemory(true);
    }

    [MustDisposeResource]
    private static int TestPersistentMapEnumeration(PersistentHashMap<int, int> map)
    {
        var count = 0;
        foreach (var keyValuePair in map)
        {
            count++;
        }

        return count;
    }
        
    private static int TestDictionaryEnumeration(Dictionary<int, int> map)
    {
        var count = 0;
        foreach (var keyValuePair in map)
        {
            count++;
        }

        return count;
    } 
        
    [MustDisposeResource]
    private static int TestImmutableMapEnumeartion(ImmutableDictionary<int, int> map)
    {
        var count = 0;
        foreach (var keyValuePair in map)
        {
            count++;
        }

        return count;
    }
        
    private static int TestConcurrentDictionaryEnumeration(ConcurrentDictionary<int, int> map)
    {
        var count = 0;
        foreach (var keyValuePair in map)
        {
            count++;
        }

        return count;
    }  
}