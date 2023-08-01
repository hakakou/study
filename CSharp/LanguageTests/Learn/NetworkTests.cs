using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AwesomeAssertions;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class NetworkTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public NetworkTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void WorkingWithNetworkResources()
        {
            var url = "https://world.episerver.com/cms/?q=pagetype";
            var uri = new Uri(url);
            /*
            Scheme: https
            Port: 443
            Host: world.episerver.com
            AbsolutePath: /cms/
            Query: ?q=pagetype
            */

            IPHostEntry entry = System.Net.Dns.GetHostEntry("www.uptiv.com");
            _testOutputHelper.WriteLine(entry.AddressList[0].ToString());
        }

        static HttpClient _client1 = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(1)
        };

        [Fact]
        public async Task TestHttpClientCancellation1()
        {
            var cts = new CancellationTokenSource();
            int e = 0;

            try
            {
                using var response = await _client1
                    .GetAsync("http://support.prog.gr/debug/sleep.php?s=2", cts.Token);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                e = 1;
            }
            catch (TaskCanceledException)
            {
                e = 2;
            }

            Assert.Equal(1, e);
        }

        [Fact]
        public async Task TestError()
        {
            Func<Task> act = async () =>
            {
                using var response = await _client1.GetAsync("http://support.prog.gr/debug/error.php?e=404");

                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Something went wrong", inner: null, response.StatusCode);
                }
            };

            await act.Should().ThrowAsync<HttpRequestException>().WithMessage("*Something*");

        }

        [Fact]
        public async Task TestHttpClientCancellation2()
        {
            var cts = new CancellationTokenSource();
            int e = 0;

            try
            {
                _ = Task.Factory.StartNew(async () =>
                  {
                      await Task.Delay(500);
                      cts.Cancel();
                  });

                using var response = await _client1
                    .GetAsync("http://support.prog.gr/debug/sleep.php?s=2", cts.Token);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                e = 1;
            }
            catch (TaskCanceledException)
            {
                e = 2;
            }

            Assert.Equal(2, e);
        }

    }
}
