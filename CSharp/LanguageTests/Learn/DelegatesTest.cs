using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;


namespace LanguageTests
{
    public class ExecuteDumb
    {
        public EventHandler Shout;
        public EventHandler<int> ShoutInt;

        public void Execute()
        {
            // Exec
            Shout?.Invoke(this, EventArgs.Empty);
            ShoutInt?.Invoke(this, 1);
        }
    }

    public class DelegatesTest
    {
        public ITestOutputHelper OutputHelper { get; }
        public DelegatesTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public delegate (int p1, int p2) DelegateIntString(int i, string p);

        public void Execute(DelegateIntString d)
        {
            d(1, "a");
        }

        [Fact]
        public void DelegateTest()
        {
            Execute(Proc);

            DelegateIntString d = Proc;
            var r = d(10, "P");

            (int p1, int p2) Proc(int i, string p)
            {
                return (i, p.GetHashCode());
            }
        }

        [Fact]
        public void EventHandlerTest()
        {
            var d = new ExecuteDumb();
            d.Shout += (s, e) => OutputHelper.WriteLine(new { s, e }.ToString());
            d.ShoutInt += (s, eInt) => OutputHelper.WriteLine(new { s, eInt }.ToString());
            d.Execute();
        }
    }

}
