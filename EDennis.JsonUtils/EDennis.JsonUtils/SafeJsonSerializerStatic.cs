using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EDennis.JsonUtils {

    public partial class SafeJsonSerializer {

        public static string Serialize<T>(T obj, int maxDepth = 99, bool indented = true, string[] propertiesToIgnore = null, bool textOrderArrayElements = false) {
            var jsonWriterOptions = new JsonWriterOptions { Indented = indented };
            using var stream = new MemoryStream();
            using var jw = new Utf8JsonWriter(stream, jsonWriterOptions);
            Serialize(obj, null, jw, maxDepth, propertiesToIgnore ?? new string[] { }, new List<int> { }, textOrderArrayElements);
            jw.Flush();
            string json = Encoding.UTF8.GetString(stream.ToArray());
            return json;
        }


        protected static void Serialize<T>(T obj, string propertyName, Utf8JsonWriter jw, int maxDepth, string[] propertiesToIgnore, List<int> hashCodes, bool textOrderArrayElements, bool isContainerType = false) {
            if (jw.CurrentDepth > maxDepth)
                return;
            if (propertiesToIgnore.Contains(propertyName))
                return;

            var jsonValueType = GetJsonValueKind(obj);
            if (jsonValueType == JsonValueKind.Array || jsonValueType == JsonValueKind.Object) {
                var hashCode = obj.GetHashCode();
                if (hashCodes.Contains(hashCode))
                    return;
                hashCodes.Add(hashCode);
            }

            if (isContainerType && jsonValueType == JsonValueKind.Null)
                return;

            if (propertyName != null) {
                jw.WritePropertyName(propertyName);
                //Debug.WriteLine($"jw.WritePropertyName({propertyName})");
            }


            if (obj != null && obj.GetType().IsEnum) {
                jw.WriteStringValue(Enum.GetName(obj.GetType(),obj));
                //Debug.WriteLine($"jw.WriteStringValue({Enum.GetName(obj.GetType(),obj)})");
                return;
            }

            switch (jsonValueType) {
                case JsonValueKind.Undefined:
                    if (propertyName == null)
                        return;
                    jw.WriteNullValue();
                    //Debug.WriteLine($"jw.WriteNullValue()");
                    break;
                case JsonValueKind.Null:
                    jw.WriteNullValue();
                    //Debug.WriteLine($"jw.WriteNullValue()");
                    break;
                case JsonValueKind.True:
                    jw.WriteBooleanValue(true);
                    //Debug.WriteLine($"jw.WriteBooleanValue(true)");
                    break;
                case JsonValueKind.False:
                    jw.WriteBooleanValue(false);
                    Debug.WriteLine($"jw.WriteBooleanValue(false)");
                    break;
                case JsonValueKind.String:
                    var result = JsonSerializer.Serialize(obj).Replace("\u0022", "");
                    jw.WriteStringValue(result);
                    //Debug.WriteLine($"jw.WriteStringValue({result})");
                    break;
                case JsonValueKind.Number:
                    var num = Convert.ToDecimal(obj);
                    jw.WriteNumberValue(num);
                    //Debug.WriteLine($"jw.WriteNumberValue({num})");
                    break;
                case JsonValueKind.Array:
                    jw.WriteStartArray();
                    //Debug.WriteLine($"jw.WriteStartArray()");
                    try {
                        var oList = (obj as IEnumerable<object>).ToList();
                        SerializeEnumerable(oList, propertyName, jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements);
                    } catch {

                        //upon failure, use reflection and generic SerializeEnumerable method
                        Type[] args = obj.GetType().GetGenericArguments();
                        Type itemType = args[0];

                        MethodInfo method = typeof(SafeJsonSerializer).GetMethod("SerializeEnumerable", BindingFlags.Static | BindingFlags.NonPublic);
                        MethodInfo genericM = method.MakeGenericMethod(itemType);
                        genericM.Invoke(null, new object[] { obj, propertyName, jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements });
                    }
                    jw.WriteEndArray();
                    //Debug.WriteLine($"jw.WriteEndArray()");
                    break;
                case JsonValueKind.Object:
                    jw.WriteStartObject();
                    //Debug.WriteLine($"jw.WriteStartObject()");
                    var type = obj.GetType();
                    if (type.IsIDictionary()) {
                        var dict = obj as IDictionary;
                        foreach (var key in dict.Keys)
                            Serialize(dict[key], key.ToString(), jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements);
                    } else {
                        foreach (var prop in type.GetProperties().Where(t=>t.DeclaringType.FullName != "System.Linq.Dynamic.Core.DynamicClass")) {
                            //try {
                                var containerType = IsContainerType(prop.PropertyType);
                                Serialize(prop.GetValue(obj), prop.Name, jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements, containerType);
                            //} catch { }
                        }
                    }
                    jw.WriteEndObject();
                    //Debug.WriteLine($"jw.WriteEndObject()");
                    break;
                default:
                    return;
            }
        }

        protected static void SerializeEnumerable<T>(IEnumerable<T> obj, string propertyName, Utf8JsonWriter jw, int maxDepth, string[] propertiesToIgnore, List<int> hashCodes, bool textOrderArrayElements = false) {
            if (textOrderArrayElements) {
                Dictionary<string, T> dict = new Dictionary<string, T>();
                foreach (var item in obj) {
                    using var stream2 = new MemoryStream();
                    using var jw2 = new Utf8JsonWriter(stream2);
                    Serialize(item, null, jw2, maxDepth - jw.CurrentDepth, propertiesToIgnore, new List<int>(), textOrderArrayElements);
                    jw2.Flush();
                    string json = Encoding.UTF8.GetString(stream2.ToArray());
                    dict.Add(json, item);
                }
                var ordered = dict.OrderBy(x => x.Key).Select(x => x.Value);
                foreach (var item in ordered)
                    Serialize(item, null, jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements);
            } else {
                foreach (var item in obj)
                    Serialize(item, null, jw, maxDepth, propertiesToIgnore, hashCodes, textOrderArrayElements);
            }
        }


        public static JsonValueKind GetJsonValueKind(object obj) {
            if (obj == null)
                return JsonValueKind.Null;
            var type = obj.GetType();
            if (type.IsArray)
                return JsonValueKind.Array;
            else if (type.IsIDictionary())
                return JsonValueKind.Object;
            else if (type.IsIEnumerable())
                return JsonValueKind.Array;
            else if (type.IsNumber())
                return JsonValueKind.Number;
            else if (type == typeof(bool)) {
                var bObj = (bool)obj;
                if (bObj)
                    return JsonValueKind.True;
                else
                    return JsonValueKind.False;
            } else if (type == typeof(string) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type.IsPrimitive
                )
                return JsonValueKind.String;
            else if ((type.GetProperties()?.Length ?? 0) > 0)
                return JsonValueKind.Object;
            else
                return JsonValueKind.Undefined;
        }


        public static bool IsContainerType(Type type) {
            if (type.IsArray)
                return true;
            else if (type.IsIDictionary())
                return true;
            else if (type.IsIEnumerable())
                return true;
            else if (type.IsNumber())
                return false;
            else if (type == typeof(bool)) {
                return false;
            } else if (type == typeof(string) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type.IsPrimitive
                )
                return false;
            else if ((type.GetProperties()?.Length ?? 0) > 0)
                return true;
            else if (type == typeof(object))
                return true;
            else
                return false;
        }
    }

    internal enum ContainerType {
        None,
        Object,
        Array
    }

    internal static class TypeExtensions {
        internal static bool IsIEnumerable(this Type type) {
            return type != typeof(string) && type.GetInterfaces().Contains(typeof(IEnumerable));
        }
        internal static bool IsIDictionary(this Type type) {
            return 
                type.GetInterfaces().Contains(typeof(IDictionary))
                || (type.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }
        internal static bool IsNumber(this Type type) {
            return type == typeof(byte)
                || type == typeof(ushort)
                || type == typeof(short)
                || type == typeof(uint)
                || type == typeof(int)
                || type == typeof(ulong)
                || type == typeof(long)
                || type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(float)
                ;
        }

    }
}