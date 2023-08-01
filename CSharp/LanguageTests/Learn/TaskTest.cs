using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class TestClass
    {
        public TestClass()
        {
        }

        public virtual TaskAwaiter<string> GetAwaiter()
        {
            return Task.Delay(1000)
                .ContinueWith<string>(task => "RESULT")
                .GetAwaiter();
        }
    }

    public class TaskTest
    {
        public ITestOutputHelper OutputHelper { get; }

        public TaskTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            Recorder.Log += (s, e) => outputHelper.WriteLine(e);
        }

        [Fact]
        public async Task Block1()
        {
            BlockingCollection<int> bCollection = new BlockingCollection<int>(boundedCapacity: 2);
            bCollection.Add(1);
            bCollection.Add(2);

            if (bCollection.TryAdd(3, TimeSpan.FromSeconds(1)))
                Assert.True(false);
            else
                OutputHelper.WriteLine("Item not added");

            var item = bCollection.Take();
            Assert.Equal(1, item);

            item = bCollection.Take();

            if (!bCollection.TryTake(out int item2, 1000))
                OutputHelper.WriteLine("Cannot take item");
            else
                Assert.True(false);
        }

        [Fact]
        public Task Block2()
        {
            var bCollection = new BlockingCollection<int>(boundedCapacity: 10);

            Recorder.Start();

            var producerThread = Task.Factory.StartNew(() =>
            {
                Recorder.Report("Producer start");
                for (int i = 0; i < 10; ++i)
                {
                    bCollection.Add(i);
                    Thread.Sleep(100);
                }
                Recorder.Report("Producer loop ended");
                bCollection.CompleteAdding();
            });

            var consumerThread = Task.Factory.StartNew(() =>
            {
                Recorder.Report("Consumer start");
                while (!bCollection.IsCompleted)
                {
                    int item = bCollection.Take();
                    OutputHelper.WriteLine(item.ToString());
                    Thread.Sleep(100);
                }
                Recorder.Report("Consumer end");
            });

            return Task.WhenAll(producerThread, consumerThread);
        }

        [Fact]
        public async Task Block4()
        {
            Recorder.Start();
            BlockingCollection<int> bCollection = new BlockingCollection<int>(boundedCapacity: 10);
            Task producerThread = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);

                for (int i = 0; i < 10; ++i)
                {
                    await Task.Delay(100);
                    bCollection.Add(i);
                }

                bCollection.CompleteAdding();
            });

            foreach (var item in bCollection.GetConsumingEnumerable())
                Recorder.Report(item.ToString());

            Recorder.Report("Finished");
        }

        //BlockingCollection<string> auctions = new BlockingCollection<string>(100);

        //Task.Run(() =>
        //{
        //    while (hasMoreAuctions)
        //    {
        //        auctions.Add(GetNewAuction);
        //    }
        //    auctions.CompleteAdding();
        //});

        //Task.Run(() =>
        //{
        //    while (!auctions.IsCompleted)
        //    {
        //        Process(auctions.Take());
        //    }
        //});

        //Task.Run(() =>
        //{
        //    foreach (var auction in auctions.GetConsumingEnumerable())
        //    {
        //        Process(auction);
        //    }
        //});

        //Parallel.ForEach(auctions.GetConsumingEnumerable(), Process);

        [Fact]
        public async Task Test4()
        {
            var stopwatch = Stopwatch.StartNew();
            string res = await new TestClass();
            OutputHelper.WriteLine($"done {res} {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task Test3()
        {
            var stopwatch = Stopwatch.StartNew();
            var tcs = new TaskCompletionSource<int>();

            OutputHelper.WriteLine($"Starting... (after {stopwatch.ElapsedMilliseconds}ms)");

            var t1 = Task.Factory.StartNew(() =>
              {
                  Thread.Sleep(1000);
              })
                .ContinueWith(task => tcs.SetResult(2));

            await tcs.Task;
            OutputHelper.WriteLine($"Waited  (after {stopwatch.ElapsedMilliseconds}ms)");

            stopwatch.Stop();
        }

        [Fact]
        public void Test1()
        {
            TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
            Task<int> t1 = tcs1.Task;

            // Start a background task that will complete tcs1.Task
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                tcs1.SetResult(15);
            });

            // The attempt to get the result of t1 blocks the current thread until the completion
            // source gets signaled. It should be a wait of ~1000 ms.
            Stopwatch sw = Stopwatch.StartNew();
            int result = t1.Result;
            sw.Stop();

            OutputHelper.WriteLine("(ElapsedTime={0}): t1.Result={1} (expected 15) ",
                sw.ElapsedMilliseconds, result);
        }

        [Fact]
        public void Test2()
        {
            // Alternatively, an exception can be manually set on a TaskCompletionSource.Task
            TaskCompletionSource<int> tcs2 = new TaskCompletionSource<int>();
            Task<int> t2 = tcs2.Task;

            // Start a background Task that will complete tcs2.Task with an exception
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                tcs2.SetException(new InvalidOperationException("SIMULATED EXCEPTION"));
            });

            // The attempt to get the result of t2 blocks the current thread until the completion
            // source gets signaled with either a result or an exception. In either case it should
            // be a wait of ~1000 ms.
            var sw = Stopwatch.StartNew();
            int result;
            try
            {
                result = t2.Result;

                OutputHelper.WriteLine("t2.Result succeeded. THIS WAS NOT EXPECTED.");
            }
            catch (AggregateException e)
            {
                OutputHelper.WriteLine("(ElapsedTime={0}): ", sw.ElapsedMilliseconds);
                OutputHelper.WriteLine(
                    "The following exceptions have been thrown by t2.Result: (THIS WAS EXPECTED)");
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    OutputHelper.WriteLine("\n---------------\n{0}", e.InnerExceptions[j].ToString());
                }
            }
        }
    }
}