using System;
using System.Security.Claims;
using System.Threading;
using Haka.Lib;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class HttpClientTests
    {
        private readonly ITestOutputHelper t;

        public HttpClientTests(ITestOutputHelper testOutputHelper)
        {
            t = testOutputHelper;
        }

        [Fact]
        public void TestSign()
        {
            Protector.Register("Alice", "Pa$$w0rd", new[] { "Admins" });
            Protector.Register("Bob", "Pa$$w0rd", new[] { "Sales", "TeamLeads" });
            Protector.Register("Eve", "Pa$$w0rd");

            Protector.LogIn("Bob", "Pa$$w0rd");
            Assert.NotNull(Thread.CurrentPrincipal);

            var p = Thread.CurrentPrincipal;
            t.WriteLine($"IsAuthenticated: {p.Identity.IsAuthenticated}");
            t.WriteLine($"AuthenticationType: {p.Identity.AuthenticationType}");
            t.WriteLine($"Name: {p.Identity.Name}");
            t.WriteLine($"IsInRole(\"Admins\"): {p.IsInRole("Admins")}");
            t.WriteLine($"IsInRole(\"Sales\"): {p.IsInRole("Sales")}");
            if (p is ClaimsPrincipal)
            {
                t.WriteLine($"{p.Identity.Name} has the following claims:");
                foreach (var claim in (p as ClaimsPrincipal).Claims)
                    t.WriteLine($"{claim.Type}: {claim.Value}");
            }
        }
    }
}
