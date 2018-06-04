using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {

    public class ColorPaletteTests {

        private readonly ITestOutputHelper _output;

        public ColorPaletteTests(ITestOutputHelper output) {
            _output = output;
        }


        [Fact]
        public void TestJsonSerialization() {
            var palette = GetColorPalette();
            var token = JToken.FromObject(palette);

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new SafeJsonConverter());

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            serializer.Serialize(sw, palette);

            _output.WriteLine(sw.ToString());
        }

        private ColorPalette GetColorPalette() {
            return new ColorPalette { Color = new string[] { "red", "black", "white" } };
        }

    }
}
