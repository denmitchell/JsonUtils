using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.JsonUtils.Tests {
    public static class ObjExtensions {

        public static string ToJsonString(this object obj) {

            string json = JsonConvert.SerializeObject(obj,
                Formatting.Indented, new SafeJsonSerializerSettings());
            return json;
        }

    }

}
