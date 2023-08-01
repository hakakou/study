using System;
using System.Diagnostics;
using System.Linq;
using Haka.Debug;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class LinqTest
    {
        public ITestOutputHelper OutputHelper { get; }

        public LinqTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void PerformanceTest()
        {
            {
                var watch = Stopwatch.StartNew();

                watch.Start();
                var numbers = Enumerable.Range(1, 2_00_000_000);
                var squares = numbers.Select(number => Math.Sin(number)).ToArray();
                watch.Stop();
                OutputHelper.WriteLine("T: " + watch.ElapsedMilliseconds);
            }

            {
                var watch = Stopwatch.StartNew();

                watch.Start();
                var numbers = Enumerable.Range(1, 2_00_000_000);
                var squares = numbers.AsParallel()
                    .Select(number => Math.Sin(number)).ToArray();
                watch.Stop();
                OutputHelper.WriteLine("P: " + watch.ElapsedMilliseconds);
            }
        }


        [Fact]
        public void GroupJoinTest()
        {
            var customers = new Customer[]
            {
            new Customer{Code = 5, Name = "Sam"},
            new Customer{Code = 6, Name = "Dave"},
            new Customer{Code = 7, Name = "Julia"},
            new Customer{Code = 8, Name = "Sue"}
            };

            // Example orders.
            var orders = new Order[]
            {
            new Order{KeyCode = 5, Product = "Book"},
            new Order{KeyCode = 6, Product = "Game"},
            new Order{KeyCode = 7, Product = "Computer"},
            new Order{KeyCode = 7, Product = "Mouse"},
            new Order{KeyCode = 8, Product = "Shirt"},
            new Order{KeyCode = 5, Product = "Underwear"}
            };

            var gj1 = customers.GroupJoin(orders,
                c => c.Code,
                o => o.KeyCode,
                (c, ordList) => new { c.Name, ordList });

            var s1 = gj1.Serialize();
            OutputHelper.WriteLine(s1);

            var gj2 = from cus in customers
                      join ord in orders on cus.Code
                        equals ord.KeyCode
                        into ordList
                      select new { cus.Name, ordList };

            var s2 = gj2.Serialize();
            OutputHelper.WriteLine(s2);
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void JoinTest()
        {
            var customers = new Customer[]
            {
            new Customer{Code = 5, Name = "Sam"},
            new Customer{Code = 6, Name = "Dave"},
            new Customer{Code = 7, Name = "Julia"},
            new Customer{Code = 8, Name = "Sue"}
            };

            // Example orders.
            var orders = new Order[]
            {
            new Order{KeyCode = 5, Product = "Book"},
            new Order{KeyCode = 6, Product = "Game"},
            new Order{KeyCode = 7, Product = "Computer"},
            new Order{KeyCode = 7, Product = "Mouse"},
            new Order{KeyCode = 8, Product = "Shirt"},
            new Order{KeyCode = 5, Product = "Underwear"}
            };

            var gj1 = customers.Join(orders,
                c => c.Code,
                o => o.KeyCode,
                (c, ord) => new { c.Name, ord });

            var s1 = gj1.Serialize();
            OutputHelper.WriteLine(s1);

            var gj2 = from cus in customers
                      join ord in orders on cus.Code
                        equals ord.KeyCode
                      select new { cus.Name, ord };

            var s2 = gj2.Serialize();
            OutputHelper.WriteLine(s2);
            Assert.Equal(s1, s2);
        }
    }

    internal class Customer
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }

    internal class Order
    {
        public int KeyCode { get; set; }
        public string Product { get; set; }
    }
}
