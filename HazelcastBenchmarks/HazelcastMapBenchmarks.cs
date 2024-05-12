using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Hazelcast;
using Hazelcast.DistributedObjects;
using static HazelcastBasics.HazelcastMapping;

namespace HazelcastBenchmarks;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
public class HazelcastMapBenchmarks
{
    private static readonly IHMap<string, int> Map = MapFactory().Result;
    
    [Benchmark]
    public async Task NoLocksBenchmark()
    {
        _ = await NoLocks(Map);
    } 
    
    [Benchmark]
    public async Task PessimisticLocksBenchmark()
    {
        _ = await PessimisticLocks(Map);
    }
    
    [Benchmark]
    public async Task OptimisticLocksBenchmark()
    {
        _ = await OptimisticLocks(Map);
    }
}