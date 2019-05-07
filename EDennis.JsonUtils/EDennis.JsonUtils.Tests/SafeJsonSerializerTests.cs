using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {


    public class SafeJsonSerializerTests {

        ITestOutputHelper _output;

        public SafeJsonSerializerTests(ITestOutputHelper output) {
            _output = output;
        }

        internal class Person2 {
            public string FirstName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public TimeSpan TimeOfBirth { get; set; }
        }

        static List<Person2> Persons = new List<Person2> {
            new Person2 { FirstName = "Jack", DateOfBirth =new DateTime(2002,2,2), TimeOfBirth=new TimeSpan(8,21,34) },
            new Person2 { FirstName = "Jill", DateOfBirth =new DateTime(2001,1,1), TimeOfBirth=new TimeSpan(9,22,35) }
        };

        [Fact]
        public void TestSerialize() {
            var json = JsonConvert.SerializeObject(Persons, new SafeJsonSerializerSettings());
            _output.WriteLine(json);
        }

    }
}
