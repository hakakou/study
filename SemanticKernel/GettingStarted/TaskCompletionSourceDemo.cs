using System;
using System.Threading.Channels;
using System.Threading.Tasks;

public class TaskCompletionSourceDemo
{
    private TaskCompletionSource<bool> _signal = new();
    private int _counter = 0;
    private readonly object _lock = new();

    public async Task RunDemo()
    {
        var producerTask = ProducerAsync();
        await ConsumerAsync();
        await producerTask;
    }

    private async Task ProducerAsync()
    {
        for (int i = 1; i <= 5; i++)
        {
            await Task.Delay(500); // Simulate work

            lock (_lock)
            {
                _counter = i;
                Console.WriteLine($"[Producer] Generated value: {i}");
                // Signal the consumer
                _signal.TrySetResult(i == 5);
            }
        }
    }

    private async Task ConsumerAsync()
    {
        bool completed = false;

        while (!completed)
        {
            // Wait for signal (outside lock!)
            completed = await _signal.Task;

            int value;
            lock (_lock)
            {
                value = _counter;
                _signal = new TaskCompletionSource<bool>();
            }

            Console.WriteLine($"[Consumer] Received value: {value}");
        }

        Console.WriteLine("[Consumer] All values received!");
    }
}

public class TaskCompletionSourceDemo2
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

    public async Task RunDemo()
    {
        Console.WriteLine("=== Multi-Consumer Demo with Channel ===\n");

        var producerTask = ProducerAsync();
        var consumer1 = ConsumerAsync("Consumer1");
        var consumer2 = ConsumerAsync("Consumer2");

        await Task.WhenAll(consumer1, consumer2, producerTask);
        Console.WriteLine("\n=== Demo Complete ===");
    }

    private async Task ProducerAsync()
    {
        for (int i = 1; i <= 5; i++)
        {
            await Task.Delay(500);
            await _channel.Writer.WriteAsync(i);
            Console.WriteLine($"[Producer] Generated value: {i}");
        }

        _channel.Writer.Complete(); // Signal completion
    }

    private async Task ConsumerAsync(string name)
    {
        await foreach (var value in _channel.Reader.ReadAllAsync())
        {
            Console.WriteLine($"[{name}] Received value: {value}");
        }

        Console.WriteLine($"[{name}] All values received!");
    }
}