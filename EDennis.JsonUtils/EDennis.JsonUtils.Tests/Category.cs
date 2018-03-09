using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.JsonUtils.Tests
{
    [JsonSimpleValue("CategoryLabel")]
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryLabel { get; set; }
    }
}
