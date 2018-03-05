using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace EDennis.JsonUtils {


    /// <summary>
    /// This class provides a JsonConverter for JSON.NET serialization.
    /// Via subclassing, this JsonConverter allows specifying the max depth
    /// for serialization and any properties to ignore during serialization.
    /// The class prevents circular referencing with the following
    /// strategy:
    ///        The same object can be serialized more than once
    ///        only at the same depth.
    /// </summary>
    public class SafeJsonConverter : JsonConverter {

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
        /// <param name="serializer">(ignored</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            return jObject.ToObject(objectType, serializer);
        }

        /// <summary>
        /// Determines if the converter can read JSON -- always true
        /// </summary>
        public override bool CanRead {
            get { return true; }
        }

        /// <summary>
        /// Determines if the converter can write JSON -- always true
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType) {
            return true;
        }


        /// <summary>
        /// Serializes an object to JSON, but prevents circular referencing
        /// </summary>
        internal class SafeJsonSerializer {

            // dictionary whose key is an object hashcode and whose value is
            // the depth of the object graph where the hashcode is registered.
            private Dictionary<int, int> _hashDictionary = new Dictionary<int, int>();

            //an optional set of property names to ignore during serialization
            private HashSet<string> _propertiesToIgnore = new HashSet<string>();

            //the maximum depth of the object graph to serialize
            private int _maxDepth = 3; //default

            //the StringBuilder and JsonWriter used for holding the serialized strings
            private StringBuilder sb = new StringBuilder();
            private JsonWriter jw;

            //the current depth of the serialized object graph
            private int depth = 0;

            /// <summary>
            /// Constructs a new SafeJsonSerializer with the provided JsonWriter
            /// </summary>
            /// <param name="jw">a JsonWriter for writing the JSON</param>
            public SafeJsonSerializer(JsonWriter jw) {
                this.jw = jw;
            }

            /// <summary>
            /// Constructs a new SafeJsonSerializer with a new JsonWriter that
            /// specifies pretty-printing.
            /// </summary>
            public SafeJsonSerializer() {
                var sw = new StringWriter(sb);
                jw = new JsonTextWriter(sw) {
                    Formatting = Formatting.Indented
                };
            }


            /// <summary>
            /// Serializes an object or array, respecting the provided settings for
            /// maximum depth and properties to ignore.
            /// </summary>
            /// <param name="obj">The object or array to serialize</param>
            /// <param name="maxDepth">The maximum depth of the object graph to serialize</param>
            /// <param name="propertiesToIgnore">An array of property names to ignore</param>
            /// <returns></returns>
            public string Serialize(object obj, int maxDepth, string[] propertiesToIgnore) {

                //save the maximum depth
                _maxDepth = maxDepth;

                //convert the string array of property names to ignore into a hash set
                foreach (string prop in propertiesToIgnore)
                    _propertiesToIgnore.Add(prop);

                //handle a list or object
                if (obj is ICollection) {
                    SerializeList(obj as IList, obj.GetHashCode(), null);
                } else {
                    SerializeObject(obj, obj.GetHashCode(), null);
                }

                //return the JSON string
                return sb.ToString();
            }


            /// <summary>
            /// Conditionally serializes a list, as long as the maximum depth is respected
            /// and no items in the list include an object that has been serialized at 
            /// a different level.
            /// </summary>
            /// <param name="list">The list to serialize</param>
            /// <param name="hashCode">The hashcode of the list</param>
            /// <param name="propertyName">The property name of the list</param>
            private void SerializeList(IList list, int hashCode, string propertyName) {

                if (list == null || list.Count == 0)
                    return;

                //don't serialize if the maximum depth is surpassed
                if (depth > _maxDepth)
                    return;

                //don't serialize if the property should be ignored
                if (propertyName != null && _propertiesToIgnore.Contains(propertyName))
                    return;

                //don't serialize if the list has already been serialized at a different level
                if (_hashDictionary.ContainsKey(hashCode) && _hashDictionary[hashCode] != depth)
                    return;

                //don't serialize if any objects in collection have already been serialized
                foreach (var obj in list) {
                    if (_hashDictionary.ContainsKey(obj.GetHashCode()) && _hashDictionary[obj.GetHashCode()] != depth)
                        return;
                }


                depth++;  //increment the depth

                //write the propertyName, if it exists
                if (propertyName != null)
                    jw.WritePropertyName(propertyName);

                //write the array
                jw.WriteStartArray();
                foreach (var obj in list)
                    SerializeObject(obj, obj.GetHashCode(), null);
                jw.WriteEndArray();

                depth--;  //decrement the depth
            }


            /// <summary>
            /// Serializes an object, as long as the maximum depth is respected
            /// and the object has not already been serialized at a different level.
            /// </summary>
            /// <typeparam name="T">The type of object to serialized</typeparam>
            /// <param name="obj">The object to serialize</param>
            /// <param name="hashCode">The hashcode of the object</param>
            /// <param name="propertyName">The property name of the object</param>
            private void SerializeObject<T>(T obj, int hashCode, string propertyName) {

                if (obj == null)
                    return;

                //don't serialize if the maximum depth is surpassed
                if (depth > _maxDepth)
                    return;

                //don't serialize if the property should be ignored
                if (propertyName != null && _propertiesToIgnore.Contains(propertyName))
                    return;

                //don't serialize if the object's hashcode has already been 
                //registered at a different depth
                if (_hashDictionary.ContainsKey(hashCode) && _hashDictionary[hashCode] != depth)
                    return;

                //if the object's hashcode is not already registered, register
                //it at the current depth
                if (!_hashDictionary.ContainsKey(hashCode))
                    _hashDictionary.Add(hashCode, depth);

                depth++; //increment the depth


                //write the propertyName, if it exists
                if (propertyName != null)
                    jw.WritePropertyName(propertyName);

                //write the object
                jw.WriteStartObject();

                var props = new Properties(obj);

                foreach (Property prop in props) {
                    //handle a collection
                    if (prop.IsCollection && prop.Value != null) {
                        Type t = prop.ElementType;
                        SerializeList(prop.Value as IList, prop.Value.GetHashCode(), prop.Name);
                        //handle a user object
                    } else if (prop.IsObject && prop.Value != null) {
                        SerializeObject(prop.Value, prop.Value.GetHashCode(), prop.Name);
                        //handle a formatted string value
                    } else if (prop.FormattedValue != null && !_propertiesToIgnore.Contains(prop.Name)) {
                        jw.WritePropertyName(prop.Name);
                        jw.WriteValue(prop.FormattedValue);
                        //handle all other values
                    } else if (!prop.IsCollection && !prop.IsObject && !_propertiesToIgnore.Contains(prop.Name)) {
                        jw.WritePropertyName(prop.Name);
                        jw.WriteValue(prop.Value);
                    }
                }
                jw.WriteEndObject();
                depth--; //decrement the depth

            }


            /// <summary>
            /// The class populates a list of Property objects
            /// associated with a provide object
            /// </summary>
            internal class Properties : List<Property> {

                /// <summary>
                /// Constructs a new Properties collection, based
                /// upon the provided object
                /// </summary>
                /// <param name="obj">An object containing properties</param>
                public Properties(object obj) {

                    //get the properties via reflection
                    var infoSource = obj.GetType().GetProperties();

                    //iterate over the property info and build
                    //the list of Property objects
                    foreach (PropertyInfo info in infoSource) {

                        // set the name and type of the property
                        var prop = new Property {
                            Name = info.Name,
                            Type = info.PropertyType
                        };

                        //update the type, if nullable 
                        if (prop.Type.IsGenericType &&
                            prop.Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                            prop.Type = Nullable.GetUnderlyingType(prop.Type);
                            prop.IsNullable = true;
                        }

                        //get the property value
                        prop.Value = info.GetValue(obj);

                        //get the formatted value, if relevant
                        string format = GetStringFormat(info);
                        prop.FormattedValue = null;
                        if (format != null)
                            prop.FormattedValue = String.Format(format, info.GetValue(obj));

                        //determine if the property is a collection, and if so, get the
                        //type of elements in the collection
                        prop.IsCollection = (prop.Type.FullName.StartsWith("System.Collections.Generic.List"));
                        if (prop.IsCollection) {
                            prop.ElementType = TypeSystem.GetElementType(prop.Type);
                        }

                        //determine if the property is a user object type
                        prop.IsObject = (prop.Type.IsClass && !prop.Type.FullName.StartsWith("System."));

                        //add the property
                        Add(prop);

                    }
                }

                /// <summary>
                /// Looks for a DisplayFormat attribute, and if found, retrieves
                /// the format as a string
                /// </summary>
                /// <param name="prop">The property that may be decorated with
                /// the [DisplayFormat] attribute</param>
                /// <returns>the format string</returns>
                private static string GetStringFormat(PropertyInfo prop) {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs) {
                        if (attr is DisplayFormatAttribute dAttr) {
                            return dAttr.DataFormatString;
                        }
                    }
                    return null;
                }
            }

            /// <summary>
            /// This class holds a property metadata
            /// </summary>
            internal class Property {
                public string Name { get; set; }
                public Type Type { get; set; }
                public Type ElementType { get; set; }
                public bool IsCollection { get; set; }
                public bool IsObject { get; set; }
                public bool IsNullable { get; set; }
                public object Value { get; set; }
                public string FormattedValue { get; set; }
            }

            /// <summary>
            /// This class provides methods for finding the element type
            /// for a collection.  This code is taken from Matt Warren's
            /// Blog post: "LINQ: Building an IQueryable Provider – Part I"
            /// (July 30, 2007)
            /// <see href="https://blogs.msdn.microsoft.com/mattwar/2007/07/30/linq-building-an-iqueryable-provider-part-i/"/>
            /// </summary>
            internal static class TypeSystem {
                internal static Type GetElementType(Type seqType) {
                    Type ienum = FindIEnumerable(seqType);
                    if (ienum == null) return seqType;
                    return ienum.GetGenericArguments()[0];
                }
                private static Type FindIEnumerable(Type seqType) {
                    if (seqType == null || seqType == typeof(string))
                        return null;
                    if (seqType.IsArray)
                        return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
                    if (seqType.IsGenericType) {
                        foreach (Type arg in seqType.GetGenericArguments()) {
                            Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                            if (ienum.IsAssignableFrom(seqType)) {
                                return ienum;
                            }
                        }
                    }
                    Type[] ifaces = seqType.GetInterfaces();
                    if (ifaces != null && ifaces.Length > 0) {
                        foreach (Type iface in ifaces) {
                            Type ienum = FindIEnumerable(iface);
                            if (ienum != null) return ienum;
                        }
                    }
                    if (seqType.BaseType != null && seqType.BaseType != typeof(object)) {
                        return FindIEnumerable(seqType.BaseType);
                    }
                    return null;
                }
            }
        }

    }
}
