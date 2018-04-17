using Xunit;
using Xunit.Abstractions;
using EDennis.JsonUtils.Tests;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace NutsAndBolts.Tests {
    public class BadPartTests {

        private readonly ITestOutputHelper output;

        public BadPartTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Theory]
        [InlineData(1)]
        public void BadFormat(int id) {

            var jsonFile = $"PartRepo\\GetById\\expected{id}.json";

            BadPart part = JToken.Parse(File.ReadAllText(jsonFile)).ToObject<BadPart>();

            Exception ex = Assert.Throws<FormatException>(() => part.ToJsonString());

            Assert.Equal("The format specified for BadPart.Weight ({0, n2}) is invalid.  Please check the syntax.", ex.Message);


        }


    }
}
