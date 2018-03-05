using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {
    public class IgnorePropertyTests {
        private readonly ITestOutputHelper output;

        public IgnorePropertyTests(ITestOutputHelper output) {
            this.output = output;
        }

        List<List<Person>> TestData = new List<List<Person>> {
            new List<Person>{
                new Person {FirstName="Bob", LastName="Barker"},
                new Person {FirstName="Monty", LastName="Hall"}
            },
            new List<Person>{
                new Person {FirstName="Drew", LastName="Carey", SysStart=new DateTime(2018,2,1)},
                new Person {FirstName="Wink", LastName="Martindale", SysStart=new DateTime(2018,2,1)}
            }
        };

        string[][] Filter = new string[][] {
            new string [] {"SysStart","SysEnd"},
            new string [] {"SysEnd"}
        };


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void IgnoreSysDates(int index) {
            var actual = TestData[index];
            var filter = Filter[index];
            var expectedJsonFile = $"IgnoreProperty\\IgnoreSysDates\\expected{index + 1}.json";

            var expectedJson = File.ReadAllText(expectedJsonFile);
            var expected = JToken.Parse(expectedJson).ToObject<List<Person>>();

            string json1 = JsonConvert.SerializeObject(actual,
                    Formatting.Indented, new SafeJsonConverter() {
                    PropertiesToIgnore = filter
                });
            string json2 = JsonConvert.SerializeObject(expected,
                    Formatting.Indented, new SafeJsonConverter() {
                    PropertiesToIgnore = filter
                });

            output.WriteLine("actual:\n" + json1);

            Assert.Equal(json1, json2);

        }

    }
}
