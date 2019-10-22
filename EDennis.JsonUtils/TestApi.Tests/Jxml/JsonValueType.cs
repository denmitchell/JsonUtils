using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Json
{
    /// <summary>
    /// Holds the type of data parsed by a JsonTextReader
    /// </summary>
    public class JsonValueType {
        public const string ARRAY = "array";
        public const string OBJECT = "object";
        public const string BOOLEAN = "boolean";
        public const string BYTES = "bytes";
        public const string DATE = "date";
        public const string DECIMAL = "decimal";
        public const string INTEGER = "integer";
        public const string STRING = "string";
        public const string NULL = "null";
    }
}
