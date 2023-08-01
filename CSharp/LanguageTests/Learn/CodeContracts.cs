using System;
using System.Diagnostics.Contracts;
using Xunit;

namespace LanguageTests
{
    // Are Code Contracts going to be supported in .NET Core going forwards? 
    // https://github.com/dotnet/docs/issues/6361

    [Obsolete("Code Contracts are not supported in .NET Core and later versions.")]
    public class CodeContracts
    {

        // [Fact]
        public void Test1()
        {
            Exec1(null);
            Exec2(null);
        }

        public void Exec1(string x)
        {
            Contract.Requires(x != null);
        }

        public void Exec2(string x)
        {
            Contract.Requires<ArgumentNullException>(x != null, "x");
        }

    }
}
