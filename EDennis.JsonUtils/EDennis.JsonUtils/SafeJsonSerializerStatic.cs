using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace EDennis.JsonUtils {

    public partial class SafeJsonSerializer {

        public static string Serialize(object obj, int maxDepth = 99, bool indented = true, string[] propertiesToIgnore = null) {
            var jsonWriterOptions = new JsonWriterOptions { Indented = indented };
            using var stream = new MemoryStream();
            using var jw = new Utf8JsonWriter(stream, jsonWriterOptions);
            Serialize(obj, null, jw, maxDepth, propertiesToIgnore ?? new string[] { }, new List<int> { }, stream);
            jw.Flush();
            string json = Encoding.UTF8.GetString(stream.ToArray());
            return json;
        }

        protected static void Serialize(object obj, string propertyName, Utf8JsonWriter jw, int maxDepth, string[] propertiesToIgnore, List<int> hashCodes, MemoryStream stream) {
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



            if (propertyName != null)
                jw.WritePropertyName(propertyName);

            switch (jsonValueType) {
                case JsonValueKind.Undefined:
                    if (propertyName == null)
                        return;
                    else
                        jw.WriteNullValue();
                    break;
                case JsonValueKind.Null:
                    jw.WriteNullValue();
                    break;
                case JsonValueKind.True:
                    jw.WriteBooleanValue(true);
                    break;
                case JsonValueKind.False:
                    jw.WriteBooleanValue(false);
                    break;
                case JsonValueKind.String:
                    jw.WriteStringValue(JsonSerializer.Serialize(obj).Replace("\u0022", ""));
                    break;
                case JsonValueKind.Number:
                    jw.WriteNumberValue(Convert.ToDecimal(obj));
                    break;
                case JsonValueKind.Array:
                    jw.WriteStartArray();
                    try {
                        var oList = (obj as IEnumerable<object>).ToList();
                        foreach (var item in oList)
                            Serialize(item, null, jw, maxDepth, propertiesToIgnore, hashCodes, stream);
                    } catch {
                        using var jw2 = new StreamWriter(stream,null,-1,true);
                        jw2.Write(JsonSerializer.Serialize(obj, new JsonSerializerOptions { MaxDepth = maxDepth }));
                    }
                    jw.WriteEndArray();
                    break;
                case JsonValueKind.Object:
                    jw.WriteStartObject();
                    var type = obj.GetType();
                    if (type.IsIDictionary()) {
                        var dict = obj as IDictionary;
                        foreach (var key in dict.Keys)
                            Serialize(dict[key], key.ToString(), jw, maxDepth, propertiesToIgnore, hashCodes, stream);
                    } else {
                        foreach (var prop in type.GetProperties())
                            Serialize(prop.GetValue(obj), prop.Name, jw, maxDepth, propertiesToIgnore, hashCodes, stream);
                    }
                    jw.WriteEndObject();
                    break;
                default:
                    return;
            }
        }



        public static JsonValueKind GetJsonValueKind(object obj) {
            var type = obj.GetType();
            if (obj == null)
                return JsonValueKind.Null;
            else if (type.IsArray)
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
    }

    internal static class TypeExtensions {
        internal static bool IsIEnumerable(this Type type) {
            return type.IsGenericType && type.GetInterfaces().Contains(typeof(IEnumerable));
        }
        internal static bool IsIDictionary(this Type type) {
            return type.IsGenericType &&
                (type.GetInterfaces().Contains(typeof(IDictionary))
                || typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()));
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