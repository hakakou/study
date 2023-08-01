using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class Person
    {
        public Person()
        {
        }

        public Person(decimal initialSalary)
        {
            Salary = initialSalary;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public HashSet<Person> Children { get; set; }
        protected decimal Salary { get; set; }
    }


    public class FileTests
    {
        List<Person> people = new List<Person> {
            new Person(20000M) { FirstName = "Alice", LastName = "Smith",
                DateOfBirth = new DateTime(1974, 3, 14) },
            new Person(40000M) { FirstName = "Bob", LastName = "Jones",
                DateOfBirth = new DateTime(1969, 11, 23) },
            new Person(20000M) { FirstName = "Charlie", LastName = "Cox",
                DateOfBirth = new DateTime(1984, 5, 4), Children = new HashSet<Person>
                    { new Person(0M) { FirstName = "Sally", LastName = "Cox",
                        DateOfBirth = new DateTime(2000, 7, 12) } }
                    }
        };


        private readonly ITestOutputHelper _testOutputHelper;

        public FileTests(ITestOutputHelper testOutputHelper)
        {
            people[2].Children.Add(people[0]);
            people[2].Children.Add(people[1]);

            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestFile()
        {
            StreamWriter textWriter = File.CreateText(@"c:/unzip/1a.txt");
            textWriter.WriteLine("Hello, C#!");
            textWriter.Close();

            StreamReader textReader = File.OpenText(@"c:/unzip/1a.txt");
            var str = textReader.ReadToEnd();
            textReader.Close();

            _testOutputHelper.WriteLine(Path.GetRandomFileName());
            _testOutputHelper.WriteLine(Path.GetTempFileName());

            File.Open(@"c:/unzip/1a.txt", FileMode.Open, FileAccess.Read, FileShare.None);
        }

        [Fact]
        public void XmlStream()
        {
            using (var fs = File.Create(@"c:/unzip/1a.xml"))
            using (var xml = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("test");
                foreach (var i in Enumerable.Range(0, 10))
                    xml.WriteElementString("num", i.ToString());
                xml.WriteEndElement();
            }

            using (var fs = File.OpenRead(@"c:/unzip/1a.xml"))
            using (var reader = XmlReader.Create(fs))
            {
                while (reader.Read())
                    _testOutputHelper.WriteLine(reader.NodeType + " " + reader.Value);
            }
        }

        [Fact]
        public void BrotliTest()
        {
            using (var fs = File.Create(@"c:/unzip/1a.bin"))
            using (var compression = new BrotliStream(fs, CompressionMode.Compress))
            using (var txt = new StreamWriter(compression))
                txt.WriteLine(new string('A', 100));
        }

        [Fact]
        public async Task XmlSerializers()
        {
            var xs = new XmlSerializer(typeof(List<Person>));
            using (var fs = File.Create(@"c:/unzip/peo.xml"))
                xs.Serialize(fs, people);

            var js = new Newtonsoft.Json.JsonSerializer();
            using (var fs = File.CreateText(@"c:/unzip/peo1.json"))
                js.Serialize(fs, people);

            using (var fs = File.Create(@"c:/unzip/peo2.json"))
                await System.Text.Json.JsonSerializer.SerializeAsync(fs, people);
        }

    }
}