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

        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with default maximum depth (99), no property filters,
        /// and ReferenceLoopHandling.Ignore.
        /// </summary>
        public SafeJsonSerializerSettings() {
            Converters = new[] { new SafeJsonConverter() };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with the provided maximum depth, the provided
        /// property filters, and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph to serialize</param>
        /// <param name="propertiesToIgnore">array of properties to ignore during serialization</param>
        public SafeJsonSerializerSettings(int maxDepth, string[] propertiesToIgnore) {
            Converters = new[] { new SafeJsonConverter(maxDepth,propertiesToIgnore) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }


        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with the provided maximum depth, the provided
        /// property filters, modulo transform, and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph to serialize</param>
        /// <param name="propertiesToIgnore">array of properties to ignore during serialization</param>
        /// <param name="moduloTransform">map or properties and modulus values for modulo tranform</param>
        public SafeJsonSerializerSettings(int maxDepth, string[] propertiesToIgnore, 
            Dictionary<string,ulong> moduloTransform) {
            Converters = new[] { new SafeJsonConverter(maxDepth, propertiesToIgnore,moduloTransform) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }


        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with the provided maximum depth, no
        /// property filters, and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph to serialize</param>
        public SafeJsonSerializerSettings(int maxDepth) {
            Converters = new[] { new SafeJsonConverter(maxDepth) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with default maximum depth (99), the provided
        /// property filters, and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="propertiesToIgnore">array of properties to ignore during serialization</param>
        public SafeJsonSerializerSettings(string[] propertiesToIgnore) {
            Converters = new[] { new SafeJsonConverter(propertiesToIgnore) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with default maximum depth (99), the provided
        /// property filters, modulo transform and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="propertiesToIgnore">array of properties to ignore during serialization</param>
        /// <param name="moduloTransform">map or properties and modulus values for modulo tranform</param>
        public SafeJsonSerializerSettings(string[] propertiesToIgnore, 
            Dictionary<string,ulong> moduloTransform) {
            Converters = new[] { new SafeJsonConverter(propertiesToIgnore, moduloTransform) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }


        /// <summary>
        /// Constructs a new SafeJsonSerializerSettings instance
        /// with default maximum depth (99), the provided
        /// modulo transform and ReferenceLoopHandling.Ignore.
        /// </summary>
        /// <param name="moduloTransform">map or properties and modulus values for modulo tranform</param>
        public SafeJsonSerializerSettings(Dictionary<string, ulong> moduloTransform) {
            Converters = new[] { new SafeJsonConverter(moduloTransform) };
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }


    }
}
