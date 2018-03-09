using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {
    public class JsonSimpleValueAttributeTests {
        private readonly ITestOutputHelper output;

        public JsonSimpleValueAttributeTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void TestItemCategory() {
            var items = new List<Item>() {
                new Item { ItemId = 1, Category = new Category { CategoryId = 999, CategoryLabel = "Extra-Large"} },
                new Item { ItemId = 2, Category = new Category { CategoryId = 888, CategoryLabel = "Large"} },
                new Item { ItemId = 3, Category = new Category { CategoryId = 777, CategoryLabel = "Medium"} },
                new Item { ItemId = 4, Category = new Category { CategoryId = 666, CategoryLabel = "Small"} },
            };

            var json = items.ToJsonString();

            output.WriteLine(json);

        }
    }
}
