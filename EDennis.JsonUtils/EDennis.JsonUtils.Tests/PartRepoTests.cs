using Xunit;
using Xunit.Abstractions;
using EDennis.JsonUtils.Tests;
using Newtonsoft.Json.Linq;
using System.IO;

namespace NutsAndBolts.Tests {
    public class PartRepoTests {

        private readonly ITestOutputHelper output;

        public PartRepoTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void PartRepoGetById(int id) {
            var expectedJsonFile = $"PartRepo\\GetById\\expected{id}.json";

            NutsAndBoltsContext context = ContextFactory.GetContext();
            var repo = new PartRepo(context);

            Part actual = repo.GetById(id);
            Part expected = JToken.Parse(File.ReadAllText(expectedJsonFile)).ToObject<Part>();

            string actualJson = actual.ToJsonString();
            string expectedJson = expected.ToJsonString();

            string sideBySide = FileStringComparer.GetSideBySideFileStrings(expectedJson, actualJson, "EXPECTED", "ACTUAL");
            output.WriteLine(sideBySide);

            Assert.Equal(expectedJson, actualJson);

        }


    }
}
