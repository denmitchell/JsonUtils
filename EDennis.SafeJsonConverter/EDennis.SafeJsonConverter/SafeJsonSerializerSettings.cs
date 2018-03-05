using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.JsonUtils {

    /// <summary>
    /// This class provides safe JSON serialization -- avoiding
    /// circular referencing, but providing reasonably complete object
    /// graphs.
    /// </summary>
    public class SafeJsonSerializerSettings : JsonSerializerSettings {

        public SafeJsonSerializerSettings() {
            Converters = new[] { new SafeJsonConverter() };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

    }
}
