using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class PerformanceTest
    {
        public ITestOutputHelper OutputHelper { get; }

        public PerformanceTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            Recorder.Log += (s, e) => outputHelper.WriteLine(e);
        }

        [Fact]
        public void Test1()
        {
            Recorder.Start();

            // simulate a process that requires some memory resources...
            int[] largeArrayOfInts = Enumerable.Range(1, 10_000).ToArray();

            // ...and takes some time to complete
            System.Threading.Thread.Sleep(new Random().Next(5, 10) * 1000);
            Recorder.Stop();
        }

        [Fact]
        public void TestStrings()
        {
            int[] numbers = Enumerable.Range(1, 50_000).ToArray();

            Recorder.Start("String with concat");
            string s = "";
            for (int i = 0; i < numbers.Length; i++)
            {
                s += numbers[i] + ", ";
            }
            Recorder.Stop();

            Recorder.Start("String with sb");
            s = "";
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < numbers.Length; i++)
            {
                builder.Append(numbers[i]); builder.Append(", ");
            }
            s = builder.ToString();
            s.ToString();
            Recorder.Stop();
        }

        [Fact]
        public void TestThreads()
        {
            Task taskA = new Task(MethodA);
            taskA.Start();
            Task taskB = Task.Factory.StartNew(MethodB);
            Task taskC = Task.Run(new Action(MethodC));

            Task[] tasks = { taskA, taskB, taskC };
            Task.WaitAll(tasks);
        }

        [Fact]
        public void TestCont()
        {
            var t = Task.Factory.StartNew(MethodA)
                .ContinueWith(a => MethodB())
                .ContinueWith(b => MethodC());

            t.Wait();
        }

        [Fact]
        public void TestNested()
        {
            var t = Task.Factory.StartNew(OuterMethod,
                 TaskCreationOptions.AttachedToParent);
            t.Wait();

            void OuterMethod()
            {
                OutputHelper.WriteLine("Outer method starting...");
                var inner = Task.Factory.StartNew(InnerMethod,
                    TaskCreationOptions.AttachedToParent);
                OutputHelper.WriteLine("Outer method finished.");
            }
            void InnerMethod()
            {
                OutputHelper.WriteLine("Inner method starting...");
                Thread.Sleep(2000);
                OutputHelper.WriteLine("Inner method finished.");
            }
        }

        [Fact]
        public void TestMonitor()
        {
            object conch = new object();
            string Message = "";
            Random r = new Random();

            Task a = Task.Factory.StartNew(methodA);
            Task b = Task.Factory.StartNew(methodB);
            Task.WaitAll(new Task[] { a, b });
            OutputHelper.WriteLine(Message);

            void methodA()
            {
                try
                {
                    Monitor.Enter(conch);
                    for (int i = 0; i < 25; i++)
                    {
                        Thread.Sleep(r.Next(200));
                        Message += "A";
                    }
                }
                finally
                {
                    Monitor.Exit(conch);
                }
            }

            void methodB()
            {
                lock (conch)
                    for (int i = 0; i < 25; i++)
                    {
                        Thread.Sleep(r.Next(200));
                        Message += "B";
                    }
            }
        }

        [Fact]
        public async Task TestAsyncEnum()
        {
            await foreach (var i in Gen())
                OutputHelper.WriteLine(i.ToString());
        }

        private async IAsyncEnumerable<int> Gen()
        {
            foreach (var i in Enumerable.Repeat(1, 10))
            {
                await Task.Delay(11).ConfigureAwait(false);
                yield return i;
            }
        }

        private void MethodA()
        {
            OutputHelper.WriteLine("Starting Method A...");
            Thread.Sleep(3000); // simulate three seconds of work
            OutputHelper.WriteLine("Finished Method A.");
        }

        private void MethodB()
        {
            OutputHelper.WriteLine("Starting Method B...");
            Thread.Sleep(2000); // simulate two seconds of work
            OutputHelper.WriteLine("Finished Method B.");
        }

        private void MethodC()
        {
            OutputHelper.WriteLine("Starting Method C...");
            Thread.Sleep(1000); // simulate one second of work
            OutputHelper.WriteLine("Finished Method C.");
        }
    }
}
