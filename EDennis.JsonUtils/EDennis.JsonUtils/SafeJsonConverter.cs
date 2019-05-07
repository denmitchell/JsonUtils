using Newtonsoft.Json;
using System;

namespace EDennis.JsonUtils {


    /// <summary>
    /// This class provides a JsonConverter for JSON.NET serialization.
    /// Via subclassing, this JsonConverter allows specifying the max depth
    /// for serialization and any properties to ignore during serialization.
    /// The class prevents circular referencing with the following
    /// strategy:
    ///        (1) A leaf node in the object graph (an object with no navigation
    ///        properties) can be serialized up to the maximum depth specified;
    ///                
    ///        OTHERWISE,
    ///        (2) The same object can be serialized more than once
    ///        only at the same depth.  
    /// </summary>
    public partial class SafeJsonConverter : JsonConverter {

        public const int DEFAULT_MAXDEPTH = 99;

        /// <summary>
        /// Constructs a new SafeJsonConverter with the provided
        /// maximum depth and properties to ignore
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph</param>
        /// <param name="propertiesToIgnore">Names of properties to ignore in object graph (this is a global ignore)</param>
        public SafeJsonConverter(int maxDepth, string[] propertiesToIgnore) {
            MaxDepth = maxDepth;
            PropertiesToIgnore = propertiesToIgnore;
        }

        /// <summary>
        /// Constructs a new SafeJsonConverter with the provided
        /// maximum depth
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph</param>
        public SafeJsonConverter(int maxDepth) {
            MaxDepth = maxDepth;
            PropertiesToIgnore = new string[] { };
        }

        /// <summary>
        /// Constructs a new SafeJsonConverter with the provided
        /// array of properties to ignore.  The maximum depth is
        /// set to 99.
        /// </summary>
        /// <param name="propertiesToIgnore">Names of properties to ignore in object graph (this is a global ignore)</param>
        public SafeJsonConverter(string[] propertiesToIgnore) {
            PropertiesToIgnore = propertiesToIgnore;
            MaxDepth = DEFAULT_MAXDEPTH;
        }

        /// <summary>
        /// Constructs a new SafeJsonConverter with
        /// default values for MaxDepth (99) and 
        /// PropertiesToIgnore (empty string)
        /// </summary>
        public SafeJsonConverter() {
            MaxDepth = 99;
            PropertiesToIgnore = new string[] { };
        }

        /// <summary>
        /// The maximum depth of object graph for serialization
        /// </summary>
        public virtual int MaxDepth { get; set; }

        /// <summary>
        /// Property names that will be ignored during serialization.
        /// </summary>
        public virtual string[] PropertiesToIgnore { get; set; }




        /// <summary>
        /// Writes an object to JSON
        /// </summary>
        /// <param name="writer">The JsonWriter to use for writing</param>
        /// <param name="value">The object/array to serialize</param>
        /// <param name="serializer">(This is ignored.)</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var sjs = new SafeJsonSerializer(writer);
            sjs.Serialize(value, MaxDepth, PropertiesToIgnore);
        }

        /// <summary>
        /// Reads an object from JSON
        /// </summary>
        /// <param name="reader">The JsonReader to use for reading</param>
        /// <param name="objectType">The type of object to deserialize</param>
        /// <param name="existingValue">An existing value of the object</param>
        /// <param name="serializer">Json Serializer</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        /// <summary>
        /// Determines if the converter can read JSON -- always true
        /// </summary>
        public override bool CanRead {
            get { return false; }
        }

        /// <summary>
        /// Determines if the converter can write JSON -- always true
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType) {
            return true;
        }

    }
}
