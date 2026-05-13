using System;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LanguageTests
{
    public class Configuration
    {
        public class CObject
        {
            public string Prop { get; set; }
        }

        [Fact]
        public void Test1()
        {
            // arrange
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(@"appconfig.json",
                    optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            // act
            var cobject = new CObject();
            configuration.GetSection("Data1").Bind(cobject);

            // assert
            Assert.Equal("A", cobject.Prop);
        }
    }
}
