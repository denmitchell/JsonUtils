using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using EDennis.JsonUtils.Tests;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NutsAndBolts.Tests {
    public class SupplierRepoTests {
        private readonly ITestOutputHelper output;

        public SupplierRepoTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Theory]
        [InlineData (1)] 
        [InlineData (2)] 
        public void GetPartsForSupplier(int supplierId) {

            string expectedJsonPath = $"SupplierRepo\\GetPartsForSupplier\\expected{supplierId}.json";

            NutsAndBoltsContext context = ContextFactory.GetContext();
            SupplierRepo repo = new SupplierRepo(context);

            List<Part> actual = repo.GetPartsForSupplier(supplierId);
            List<Part> expected = JToken.Parse(File.ReadAllText(expectedJsonPath)).ToObject<List<Part>>();

            string actualJson = actual.ToJsonString();
            string expectedJson = expected.ToJsonString();

            string sideBySide = FileStringComparer.GetSideBySideFileStrings(expectedJson, actualJson, "EXPECTED", "ACTUAL");
            output.WriteLine(sideBySide);

            Assert.Equal(expectedJson, actualJson);

        }

    }
}
