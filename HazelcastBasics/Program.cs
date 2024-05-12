using Hazelcast;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;

namespace HazelcastBasics;

class HazelcastBasics
{
    public static async Task Main()
    {
        var options = new HazelcastOptionsBuilder().Build();
        await using var hz = await HazelcastClientFactory.StartNewClientAsync(options);
        var map = await hz.GetMapAsync<string, int>("my-distributed-map-2");

        //await DistributedMapExample(hz);
        //await LockExamples(map);
        await BoundedQueueExample(hz);
        
        await hz.DisposeAsync();
    }

    private static async Task BoundedQueueExample(IHazelcastClient hz)
    {
        await using var queue = await hz.GetQueueAsync<int>("bounded-queue");
        var writerTask = HazelcastQueue.WriteToQueue(queue);
        var readerTask1 = HazelcastQueue.ReadFromQueue(queue, "Reader1");
        var readerTask2 = HazelcastQueue.ReadFromQueue(queue, "Reader2");
        
        await writerTask;
        
        await Task.WhenAny(readerTask1, readerTask2, Task.Delay(TimeSpan.FromSeconds(10)));
    }

    private static async Task SoloWriterExample(IHazelcastClient hz)
    {
        await using var queue = await hz.GetQueueAsync<int>("bounded-queue");
        var writerTask = HazelcastQueue.WriteToQueue(queue);
        
        await writerTask;
    }

    private static async Task DistributedMapExample(IHazelcastClient hz)
    {
        // Get or Create a Distributed Map
        var map = await hz.GetMapAsync<int, string>("my-distributed-map");

        // Populate the map with 1000 entries
        for (var i = 0; i < 1000; i++)
        {
            await map.SetAsync(i, $"Value {i}");
        }

        Console.WriteLine("1000 entries added to the Distributed Map");
    }

    private static async Task LockExamples(IHMap<string,int> map)
    {
        Console.WriteLine($"Final value without locks: {await HazelcastMapping.NoLocks(map)}");
        Console.WriteLine($"Final value with pessimistic locking: {await HazelcastMapping.PessimisticLocks(map)}");
        Console.WriteLine($"Final value with optimistic locking: {await HazelcastMapping.OptimisticLocks(map)}");
    }
}

public class HazelcastMapping
{
    public static async Task<IHMap<string, int>> MapFactory()
    {
        var options = new HazelcastOptionsBuilder().Build();
        var hz = await HazelcastClientFactory.StartNewClientAsync(options);
        var map = await hz.GetMapAsync<string, int>("my-distributed-map-2");
        
        return map;
    }
    
    public static async Task<int> NoLocks(IHMap<string,int> map)
    {
        await map.PutAsync("key", 0);

        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
        {
            var task = Task.Run(async () =>
            {
                for (var k = 0; k < 10_000; k++)
                {
                    var currentValue = await map.GetAsync("key");
                    currentValue++;
                    await map.PutAsync("key", currentValue);
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return await map.GetAsync("key");
    }

    public static async Task<int> PessimisticLocks(IHMap<string,int> map)
    {
        await map.PutAsync("key", 0);
        
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
        {
            using (AsyncContext.New())
            {
                var task = Task.Run(async () =>
                {
                    for (var k = 0; k < 10_000; k++)
                    {
                        await map.LockAsync("key");
                        
                        try
                        {
                            var value = await map.GetAsync("key");
                            value++;
                            await map.PutAsync("key", value);
                        }
                        finally
                        {
                            await map.UnlockAsync("key");
                        }
                    }
                });

                tasks.Add(task);
            }
        }
        
        await Task.WhenAll(tasks);        

        return await map.GetAsync("key");
    }
    
    public static async Task<int> OptimisticLocks(IHMap<string,int> map)
    {
        await map.PutAsync("key", 0);
        
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
        {
            var task = Task.Run(async () =>
            {
                for (var k = 0; k < 10_000; k++)
                {
                    var success = false;
                    while (!success)
                    {
                        var oldValue = await map.GetAsync("key");
                        var newValue = oldValue + 1;
                        success = await map.ReplaceAsync("key", oldValue, newValue);
                    }
                }
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);        

        return await map.GetAsync("key");
    }
}

public class HazelcastQueue
{
    public static async Task WriteToQueue(IHQueue<int> queue) {
        for (var i = 1; i <= 100; i++) {
            await queue.PutAsync(i);
            Console.WriteLine($"Produced: {i}");
        }
    }
    
    public static async Task ReadFromQueue(IHQueue<int> queue, string readerId) {
        while (true) {
            var item = await queue.TakeAsync();
            Console.WriteLine($"{readerId} consumed: {item}");
        }
    }
}