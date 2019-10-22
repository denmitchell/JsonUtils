using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

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
    public class SafeJsonConverter : JsonConverter {

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
        /// maximum depth and properties to ignore
        /// </summary>
        /// <param name="maxDepth">Maximum depth of the object graph</param>
        /// <param name="propertiesToIgnore">Names of properties to ignore in object graph (this is a global ignore)</param>
        /// <param name="moduloTransform">Names of properties to which to apply a modulo operation (value is modulus)</param>
        public SafeJsonConverter(int maxDepth, string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform) {
            MaxDepth = maxDepth;
            PropertiesToIgnore = propertiesToIgnore;
            ModuloTransform = moduloTransform;
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
            ModuloTransform = new Dictionary<string, ulong>();
            MaxDepth = DEFAULT_MAXDEPTH;
        }


        /// <summary>
        /// Constructs a new SafeJsonConverter with the provided
        /// array of properties to ignore.  The maximum depth is
        /// set to 99.
        /// </summary>
        /// <param name="moduloTransform">Names of properties to which to apply a modulo operation (value is modulus)</param>
        public SafeJsonConverter(Dictionary<string, ulong> moduloTransform) {
            PropertiesToIgnore = new string[] { };
            ModuloTransform = moduloTransform;
            MaxDepth = DEFAULT_MAXDEPTH;
        }


        /// <summary>
        /// Constructs a new SafeJsonConverter with the provided
        /// array of properties to ignore.  The maximum depth is
        /// set to 99.
        /// </summary>
        /// <param name="propertiesToIgnore">Names of properties to ignore in object graph (this is a global ignore)</param>
        /// <param name="moduloTransform">Names of properties to which to apply a modulo operation (value is modulus)</param>
        public SafeJsonConverter(string[] propertiesToIgnore, Dictionary<string, ulong> moduloTransform) {
            PropertiesToIgnore = propertiesToIgnore;
            ModuloTransform = moduloTransform;
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
            ModuloTransform = new Dictionary<string, ulong>();
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
        /// Properties on which a modulo operation should be performed
        /// (dictionary key is the property name).
        /// as well as the modulus itself (dictionary value is
        /// the modulus).
        /// 
        /// This transform is useful for complex Ids that are generated 
        /// by adding a test-specific subsequence value to a high-start/
        /// high-increment sequence value.  The transform can produce
        /// Ids that don't change between tests, even in a shared 
        /// database environment.
        /// </summary>
        public virtual Dictionary<string,ulong> ModuloTransform { get; set; }

        /// <summary>
        /// Writes an object to JSON
        /// </summary>
        /// <param name="writer">The JsonWriter to use for writing</param>
        /// <param name="value">The object/array to serialize</param>
        /// <param name="serializer">(This is ignored.)</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var sjs = new SafeJsonSerializer(writer);
            sjs.Serialize(value, MaxDepth, PropertiesToIgnore, ModuloTransform);
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

        ///// <summary>
        ///// Serializes an object to JSON, but prevents circular referencing
        ///// </summary>
        //internal class SafeJsonSerializer {

        //    // dictionary whose key is an object hashcode and whose value is
        //    // the depth of the object graph where the hashcode is registered.
        //    private Dictionary<int, int> _hashDictionary = new Dictionary<int, int>();

        //    //an optional set of property names to ignore during serialization
        //    private HashSet<string> _propertiesToIgnore = new HashSet<string>();

        //    //an optional mapping of property names to modulus values.  When
        //    //mapped, the modulo operation will be performed.
        //    private Dictionary<string, ulong> _moduloTransform { get; set; }

        //    //the maximum depth of the object graph to serialize
        //    private int _maxDepth = 3; //default

        //    //the StringBuilder and JsonWriter used for holding the serialized strings
        //    private StringBuilder sb = new StringBuilder();
        //    private JsonWriter jw;

        //    //the current depth of the serialized object graph
        //    private int depth = 1;

        //    /// <summary>
        //    /// Constructs a new SafeJsonSerializer with the provided JsonWriter
        //    /// </summary>
        //    /// <param name="jw">a JsonWriter for writing the JSON</param>
        //    public SafeJsonSerializer(JsonWriter jw) {
        //        this.jw = jw;
        //    }

        //    /// <summary>
        //    /// Constructs a new SafeJsonSerializer with a new JsonWriter that
        //    /// specifies pretty-printing.
        //    /// </summary>
        //    public SafeJsonSerializer() {
        //        var sw = new StringWriter(sb);
        //        jw = new JsonTextWriter(sw) {
        //            Formatting = Formatting.Indented
        //        };
        //    }


        //    /// <summary>
        //    /// Serializes an object or array, respecting the provided settings for
        //    /// maximum depth and properties to ignore.
        //    /// </summary>
        //    /// <param name="obj">The object or array to serialize</param>
        //    /// <param name="maxDepth">The maximum depth of the object graph to serialize</param>
        //    /// <param name="propertiesToIgnore">An array of property names to ignore</param>
        //    /// <returns></returns>
        //    public string Serialize(object obj, int maxDepth, string[] propertiesToIgnore,
        //        Dictionary<string,ulong> moduloTransform) {

        //        //save the maximum depth
        //        _maxDepth = maxDepth;

                
        //        //convert the string array of property names to ignore into a hash set
        //        foreach (string prop in propertiesToIgnore)
        //            _propertiesToIgnore.Add(prop);

        //        //save the modulo operation map
        //        _moduloTransform = moduloTransform;


        //        //handle a list or object
        //        if (obj is ICollection || obj is IEnumerable) {
        //            SerializeList(obj as IList, obj.GetHashCode(), null);
        //        } else {
        //            SerializeObject(obj, obj.GetHashCode(), null);
        //        }

        //        //return the JSON string
        //        return sb.ToString();
        //    }


        //    /// <summary>
        //    /// Conditionally serializes a list, as long as the maximum depth is respected
        //    /// and no items in the list include an object that has been serialized at 
        //    /// a different level.
        //    /// </summary>
        //    /// <param name="list">The list to serialize</param>
        //    /// <param name="hashCode">The hashcode of the list</param>
        //    /// <param name="propertyName">The property name of the list</param>
        //    private void SerializeList(IList list, int hashCode, string propertyName) {

        //        if (list == null || list.Count == 0)
        //            return;

        //        //don't serialize if the maximum depth is surpassed
        //        if (depth > _maxDepth)
        //            return;

        //        //don't serialize if the property should be ignored
        //        if (propertyName != null && _propertiesToIgnore.Contains(propertyName))
        //            return;

        //        //don't serialize if the list has already been serialized at a different level
        //        if (_hashDictionary.ContainsKey(hashCode) && _hashDictionary[hashCode] != depth)
        //            return;

        //        //don't serialize if any objects in collection have already been serialized
        //        foreach (var obj in list) {
        //            if (obj != null && _hashDictionary.ContainsKey(obj.GetHashCode()) && _hashDictionary[obj.GetHashCode()] != depth)
        //                return;
        //        }


        //        depth++;  //increment the depth

        //        //write the propertyName, if it exists
        //        if (propertyName != null)
        //            WritePropertyName(jw, propertyName);

        //        //write the array
        //        jw.WriteStartArray();
        //        foreach (var obj in list) {
        //            if (IsSimple(obj.GetType())) {
        //                WriteValue(jw, propertyName, obj);
        //            } else {
        //                SerializeObject(obj, obj.GetHashCode(), null);
        //            }
        //        }
        //        jw.WriteEndArray();

        //        depth--;  //decrement the depth
        //    }


        //    private bool IsSimple(Type type) {
        //        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        //            // nullable type, check if the nested type is simple.
        //            return IsSimple((type.GetGenericArguments()[0]).GetTypeInfo());
        //        }
        //        return type.IsPrimitive
        //            || type.IsEnum
        //            || type.Equals(typeof(string))
        //            || type.Equals(typeof(decimal));
        //    }


        //    /// <summary>
        //    /// Serializes an object, as long as the maximum depth is respected
        //    /// and the object has not already been serialized at a different level.
        //    /// </summary>
        //    /// <typeparam name="T">The type of object to serialized</typeparam>
        //    /// <param name="obj">The object to serialize</param>
        //    /// <param name="hashCode">The hashcode of the object</param>
        //    /// <param name="propertyName">The property name of the object</param>
        //    private void SerializeObject<T>(T obj, int hashCode, string propertyName) {

        //        if (obj == null)
        //            return;

        //        //don't serialize if the maximum depth is surpassed
        //        if (depth > _maxDepth)
        //            return;

        //        //don't serialize if the property should be ignored
        //        if (propertyName != null && _propertiesToIgnore.Contains(propertyName))
        //            return;

        //        //handle primitives
        //        var type = obj.GetType();
        //        if (type.IsPrimitive || type.Name == "String"
        //            || type.Name == "Decimal" || type.Name == "Float" || type.Name == "DateTime") {

        //            //handle modulo transformations
        //            if (_moduloTransform.ContainsKey(propertyName) && 
        //                ( type.Name == typeof(int).Name || type.Name == typeof(long).Name || type.Name == typeof(uint).Name) || type.Name == typeof(ulong).Name)
        //                WriteValue(jw, propertyName, (Convert.ToUInt64(obj) % _moduloTransform[propertyName]).ToString());
        //            else
        //                WriteValue(jw, propertyName, obj.ToString());
        //            return;
        //        }


        //        var props = new Properties(obj);

        //        //if class is decorated with value property, serialize to a simple value
        //        var valueProperty = props.GetValueProperty();

        //        if (valueProperty != null) {
        //            WritePropertyName(jw, propertyName);
        //            var prop = props.Where(p => p.Name == valueProperty).FirstOrDefault();
        //            if (prop == null)
        //                throw new ArgumentOutOfRangeException(
        //                    $"JsonSimpleValue Attribute specified on {obj.GetType().Name} attempts to target an undefined property: {propertyName}.");
        //            WriteValue(jw, prop.Name, prop.Value);
        //            return;
        //        }


        //        //don't serialize if the object has navigation properties
        //        //and the object's hashcode has already been 
        //        //registered at a different depth
        //        if (props.HasNavigationProperties() 
        //            && _hashDictionary.ContainsKey(hashCode) && _hashDictionary[hashCode] != depth)
        //            return;

        //        //if the object's hashcode is not already registered, register
        //        //it at the current depth
        //        if (!_hashDictionary.ContainsKey(hashCode))
        //            _hashDictionary.Add(hashCode, depth);

        //        depth++; //increment the depth


        //        //write the propertyName, if it exists
        //        if (propertyName != null)
        //            WritePropertyName(jw, propertyName);

        //        //write the object
        //        jw.WriteStartObject();


        //        foreach (Property prop in props) {
        //            //handle a collection
        //            if ((prop.IsCollection || prop.IsArray) && prop.Value != null) {
        //                Type t = prop.ElementType;
        //                SerializeList(prop.Value as IList, prop.Value.GetHashCode(), prop.Name);
        //                //handle a user object
        //            } else if (prop.IsObject && prop.Value != null) {
        //                SerializeObject(prop.Value, prop.Value.GetHashCode(), prop.Name);
        //                //handle a formatted string value
        //            } else if (prop.FormattedValue != null && !_propertiesToIgnore.Contains(prop.Name)) {
        //                WritePropertyName(jw, prop.Name);
        //                WriteValue(jw, prop.Name, prop.FormattedValue);
        //                //handle all other values
        //            } else if (!prop.IsCollection && !prop.IsArray && !prop.IsObject && !_propertiesToIgnore.Contains(prop.Name)) {
        //                WritePropertyName(jw, prop.Name);
        //                WriteValue(jw, prop.Name, prop.Value);
        //            }
        //        }
        //        jw.WriteEndObject();
        //        depth--; //decrement the depth

        //    }

        //    private void WritePropertyName(JsonWriter jw, string propName) {
        //        try {
        //            jw.WritePropertyName(propName);
        //        } catch (JsonWriterException ex) {
        //            throw new ApplicationException($"Cannot write property name {propName} to json for {JToken.FromObject(ex.Data).ToString()} at {ex.Path}.");
        //        }
        //    }

        //    private void WriteValue<T>(JsonWriter jw, string propName, T value) {
        //        try {
        //            jw.WriteValue(value);
        //        } catch (JsonWriterException ex) {
        //            throw new ApplicationException($"Cannot write value {JToken.FromObject(value).ToString()} for property name {propName} to json for {JToken.FromObject(ex.Data).ToString()} at {ex.Path}.");
        //        }
        //    }
        //    /// <summary>
        //    /// The class populates a list of Property objects
        //    /// associated with a provide object
        //    /// </summary>
        //    internal class Properties : List<Property> {

        //        private object _object;

        //        /// <summary>
        //        /// Constructs a new Properties collection, based
        //        /// upon the provided object
        //        /// </summary>
        //        /// <param name="obj">An object containing properties</param>
        //        public Properties(object obj) {

        //            _object = obj;

        //            //get the properties via reflection
        //            var infoSource = obj.GetType().GetProperties();

        //            //iterate over the property info and build
        //            //the list of Property objects
        //            foreach (PropertyInfo info in infoSource) {

        //                // set the name and type of the property
        //                var prop = new Property {
        //                    Name = info.Name,
        //                    Type = info.PropertyType
        //                };

        //                //update the type, if nullable 
        //                if (prop.Type.IsGenericType &&
        //                    prop.Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        //                    prop.Type = Nullable.GetUnderlyingType(prop.Type);
        //                    prop.IsNullable = true;
        //                }

        //                //get the property value
        //                prop.Value = info.GetValue(obj);

        //                //get the formatted value, if relevant
        //                string format = GetStringFormat(info);
        //                prop.FormattedValue = null;
        //                if (format != null)
        //                    try {
        //                        prop.FormattedValue = String.Format(format, info.GetValue(obj));
        //                    }
        //                    catch(FormatException) {
        //                        string msg = $"The format specified for {obj.GetType().Name}.{prop.Name} ({format}) is invalid.  Please check the syntax.";
        //                        throw new FormatException(msg);
        //                    }
        //                //determine if the property is a collection, and if so, get the
        //                //type of elements in the collection
        //                prop.IsCollection = (prop.Type.FullName.StartsWith("System.Collections.Generic.List"));
        //                if (prop.IsCollection) {
        //                    prop.ElementType = TypeSystem.GetElementType(prop.Type);
        //                }

        //                prop.IsArray = prop.Type.FullName.EndsWith("[]");
        //                if (prop.IsArray) {
        //                    prop.ElementType = TypeSystem.GetElementType(prop.Type);
        //                }

        //                //determine if the property is a user object type
        //                prop.IsObject = (prop.Type.IsClass && !prop.Type.FullName.StartsWith("System."));

        //                //add the property
        //                Add(prop);

        //            }
        //        }

        //        /// <summary>
        //        /// Determines if the object has navigation properties (list or object)
        //        /// </summary>
        //        /// <returns>true if object has a list or object property; false, otherwise</returns>
        //        public bool HasNavigationProperties() {
        //            return (this.Where(p => p.IsCollection || p.IsObject).Count() > 0);
        //        }


        //        /// <summary>
        //        /// Gets the value property defined with JsonSimpleValueAttribute
        //        /// </summary>
        //        /// <returns>value property or null if JsonSimpleValueAttribute is not
        //        /// defined on the current class</returns>
        //        public string GetValueProperty() {
        //            var dnAttribute = _object.GetType().GetCustomAttributes(
        //                typeof(JsonSimpleValueAttribute), true
        //            ).FirstOrDefault() as JsonSimpleValueAttribute;
        //            if (dnAttribute != null) {
        //                return dnAttribute.ValueProperty;
        //            }
        //            return null;
        //        }


        //        /// <summary>
        //        /// Looks for a DisplayFormat attribute, and if found, retrieves
        //        /// the format as a string
        //        /// </summary>
        //        /// <param name="prop">The property that may be decorated with
        //        /// the [DisplayFormat] attribute</param>
        //        /// <returns>the format string</returns>
        //        private static string GetStringFormat(PropertyInfo prop) {
        //            object[] attrs = prop.GetCustomAttributes(true);
        //            foreach (object attr in attrs) {
        //                if (attr is DisplayFormatAttribute dAttr) {
        //                    return dAttr.DataFormatString;
        //                }
        //            }
        //            return null;
        //        }
        //    }

        //    /// <summary>
        //    /// This class holds a property metadata
        //    /// </summary>
        //    internal class Property {
        //        public string Name { get; set; }
        //        public Type Type { get; set; }
        //        public Type ElementType { get; set; }
        //        public bool IsCollection { get; set; }
        //        public bool IsArray { get; set; }
        //        public bool IsObject { get; set; }
        //        public bool IsNullable { get; set; }
        //        public object Value { get; set; }
        //        public string FormattedValue { get; set; }
        //    }

        //    /// <summary>
        //    /// This class provides methods for finding the element type
        //    /// for a collection.  This code is taken from Matt Warren's
        //    /// Blog post: "LINQ: Building an IQueryable Provider – Part I"
        //    /// (July 30, 2007)
        //    /// <see href="https://blogs.msdn.microsoft.com/mattwar/2007/07/30/linq-building-an-iqueryable-provider-part-i/"/>
        //    /// </summary>
        //    internal static class TypeSystem {
        //        internal static Type GetElementType(Type seqType) {
        //            Type ienum = FindIEnumerable(seqType);
        //            if (ienum == null) return seqType;
        //            return ienum.GetGenericArguments()[0];
        //        }
        //        private static Type FindIEnumerable(Type seqType) {
        //            if (seqType == null || seqType == typeof(string))
        //                return null;
        //            if (seqType.IsArray)
        //                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
        //            if (seqType.IsGenericType) {
        //                foreach (Type arg in seqType.GetGenericArguments()) {
        //                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
        //                    if (ienum.IsAssignableFrom(seqType)) {
        //                        return ienum;
        //                    }
        //                }
        //            }
        //            Type[] ifaces = seqType.GetInterfaces();
        //            if (ifaces != null && ifaces.Length > 0) {
        //                foreach (Type iface in ifaces) {
        //                    Type ienum = FindIEnumerable(iface);
        //                    if (ienum != null) return ienum;
        //                }
        //            }
        //            if (seqType.BaseType != null && seqType.BaseType != typeof(object)) {
        //                return FindIEnumerable(seqType.BaseType);
        //            }
        //            return null;
        //        }
        //    }
        //}

    }
}
