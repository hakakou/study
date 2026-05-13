using System;

namespace LanguageTests
{
    public class ClassWithAccessModifiers
    {
        private protected string PrivProt()
        {
            return "";
        }
        internal protected string IntProt()
        {
            return "";
        }
    }

    public class ClassWithAccessModifiersChild : ClassWithAccessModifiers
    {
        public void Test()
        {
            PrivProt();
            var c = new ClassWithAccessModifiers();
            c.IntProt();
        }
    }

}
