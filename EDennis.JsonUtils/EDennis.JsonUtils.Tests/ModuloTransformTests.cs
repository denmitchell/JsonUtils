using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {
    public class ModuloTransformTests {
        private readonly ITestOutputHelper output;

        public ModuloTransformTests(ITestOutputHelper output) {
            this.output = output;
        }

        Dictionary<string,ulong>[] ModuloTransform = new Dictionary<string, ulong>[] {
            new Dictionary<string,ulong> {{"Id",1000}},
            new Dictionary<string, ulong> { { "Id", 1000 },{"ContactId",100}}
        };

        internal class Contact {
            public int ContactId { get; set; }
            public string Email { get; set; }
        }
        internal class PersonWithContact {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Contact> ContactInfo { get; set; } 
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TestModuloTransform(int index) {

            var expectedJsonFile = $"ModuloTransformTests\\expected{index + 1}.json";
            var expectedJson = File.ReadAllText(expectedJsonFile);
            var expectedObject = JToken.Parse(expectedJson).ToObject<List<PersonWithContact>>();

            var inputJsonFile = $"ModuloTransformTests\\expected{index + 1}.json";
            var inputJson = File.ReadAllText(inputJsonFile);
            var inputObject = JToken.Parse(expectedJson).ToObject<List<PersonWithContact>>();

            var filter = ModuloTransform[index];


            string actual = JsonConvert.SerializeObject(inputObject,
                    Formatting.Indented, new SafeJsonConverter() {
                    PropertiesToIgnore = new string[] { },
                    ModuloTransform = filter
                });
            string expected = JsonConvert.SerializeObject(expectedObject,
                    Formatting.Indented, new SafeJsonConverter() {
                        PropertiesToIgnore = new string[] { },
                        ModuloTransform = filter
                    });

            output.WriteLine($"actual:\n{actual}\n\nexpected:\n{expected}" );

            Assert.Equal(expected, actual);

        }

    }
}
