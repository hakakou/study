using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnet_trace
{
    class Program
    {

        private static readonly HttpClient _client = new HttpClient();

        static async Task Main()
        {
            // No listener needed but print the process ID and wait for a key press to start the request.
            Console.WriteLine(Environment.ProcessId);
            Console.ReadKey();

            // Send an HTTP request.
            using var response = await _client.GetAsync("https://github.com/runtime");
        }
    }
}
