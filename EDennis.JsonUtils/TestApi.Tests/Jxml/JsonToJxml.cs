using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;



namespace EDennis.NetCoreTestingUtilities.Json {

    /// <summary>
    /// This class transforms JSON into a JXML structure
    /// a valid XML document with supplementary markup, which
    /// can be manipulated with XSLT and then transformed
    /// back into JSON.  The resulting structure adds XML
    /// processing instructions to denote the start and end
    /// of arrays and objects and the data types of values.
    /// For top-level arrays, arbitrary elements with a jx
    /// namespace prefix are added.  The entire XML structure
    /// is encapsulated in jx:root element.  The class makes
    /// heavy use of Newtonsoft libraries.
    /// </summary>
    public class JsonToJxml {

        //define constants
        public const string ROOT = "root";
        public const string OBJECT = "object";
        public const string VALUE = "value";
        public const string SUBSIB = "sub-sib";
        public const string TYPE = "type";
        public const string NAMESPACE_URI = "http://edennis.com/2013/jsonxml";
        public const string NAMESPACE_PREFIX = "jx";

        public const string TAG_CLEAN_PATTERN = "(^[^A-Za-z]*)|([^A-Za-z0-9\\\\-_]*)";
        private static Regex regex = new Regex(TAG_CLEAN_PATTERN);

        /// <summary>
        /// The type of node parsed
        /// </summary>
        internal enum JsonContextType {
            ROOT,
            ARRAY,
            OBJECT,
            KEY
        }

        /// <summary>
        /// Token information read by the JsonTextReader
        /// </summary>
        internal class JsonReaderToken {
            public JsonToken Type { get; set; }
            public object Value { get; set; }
        }

        /// <summary>
        /// Member type for the lineage stack.
        /// </summary>
        internal class Parent {
            public JsonContextType Type { get; set; }
            public int ChildCount { get; set; } = 1; //needed for arrays
        }

        /// <summary>
        /// Member type for the tags stack.
        /// </summary>
        internal class Tag {
            public string Prefix { get; set; } = null;
            public string Name { get; set; }
            public string URI { get; set; } = null;
        }

        /// <summary>
        /// This stack is used to hold values that will
        /// be used for element tags
        /// </summary>
        private Stack<Tag> tags = new Stack<Tag>();

        /// <summary>
        /// This stack is used to identify the parent 
        /// node type and child count.  The child count
        /// is used for arrays because the first child
        /// is handled differently from subsequent 
        /// array items.
        /// </summary>
        private Stack<Parent> lineage = new Stack<Parent>();

        /// <summary>
        /// This stack holds hashcode values for each
        /// array.  The hashcode values are used to identify
        /// the start and end of specific arrays.
        /// </summary>
        private Stack<int> arrays = new Stack<int>();

        //define a class-level writer
        XmlWriter xwriter = null;

        /// <summary>
        /// Converts Json represented by a Json.NET
        /// JToken into a JXML document, represented by
        /// a standard System.Xml.XmlDocument
        /// </summary>
        /// <param name="jToken">Valid Json.NET JToken object </param>
        /// <returns>XML document with special markup for transforming back to JSON</returns>
        public XmlDocument ConvertToJxml(JToken jToken) {

            //instantiate a new XmlDocument for holding the results
            var doc = new XmlDocument();

            //settings for formatted XML (helpful for debugging)
            var settings = new XmlWriterSettings {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true
            };

            //initialize an XmlWriter and a JsonTextReader
            using (xwriter = doc.CreateNavigator().AppendChild()) {
                using (var jreader = new JsonTextReader(new StringReader(jToken.ToString()))) {

                    //write the root element and push ROOT to the lineage
                    xwriter.WriteStartElement(NAMESPACE_PREFIX, ROOT, NAMESPACE_URI);
                    lineage.Push(new Parent { Type = JsonContextType.ROOT });

                    //read all JSON tokens, one by one
                    while (jreader.Read()) {

                        //get the current token and its value
                        var currentToken = new JsonReaderToken { Type = jreader.TokenType };

                        if (jreader.Value != null)
                            currentToken.Value = jreader.Value;


                        //handle the token differently, depending upon its type
                        switch (currentToken.Type) {

                            //handle "{"
                            case JsonToken.StartObject: {

                                    //if there is no tag, add an "jx:object" tag
                                    if (tags.Count == 0)
                                        tags.Push(new Tag { Prefix = NAMESPACE_PREFIX, Name = OBJECT, URI = NAMESPACE_URI });

                                    //if this object is a child of the root, output a start element with the current tag
                                    if (lineage.Peek().Type != JsonContextType.ROOT)
                                        xwriter.WriteStartElement(tags.Peek().Prefix, tags.Peek().Name, tags.Peek().URI);

                                    //if this object is part of an array ... 
                                    if (lineage.Peek().Type == JsonContextType.ARRAY) {
                                        //if this is a subsequent item in the same array, output a jx:sub-sib="true" attribute
                                        if (lineage.Peek().ChildCount > 1)
                                            xwriter.WriteAttributeString(NAMESPACE_PREFIX, SUBSIB, NAMESPACE_URI, "true");
                                        //output all relevant <?array ___________?> processing instructions
                                        WriteArrayItems();
                                        //increment the counter for children of this array
                                        lineage.Peek().ChildCount++;
                                    }

                                    //output a <?object-start?> processing instruction
                                    xwriter.WriteProcessingInstruction($"object-start", null);

                                    //push OBJECT to the lineage stack 
                                    lineage.Push(new Parent { Type = JsonContextType.OBJECT });

                                    break;
                                }
                            //handle "["
                            case JsonToken.StartArray: {
                                    //create an integer hash of a guid and push this code,
                                    //along with ARRAY to the lineage stack.
                                    arrays.Push(Guid.NewGuid().ToString().GetHashCode());
                                    lineage.Push(new Parent { Type = JsonContextType.ARRAY });
                                    break;
                                }
                            //handle a JSON key name
                            case JsonToken.PropertyName: {
                                    //get the key name, clean it of all XML-invalid characters
                                    //and push it to the tags stack
                                    string key = currentToken.Value.ToString();
                                    key = regex.Replace(key, "");
                                    tags.Push(new Tag { Prefix = null, Name = key, URI = null });
                                    break;
                                }
                            //handle a JSON string value
                            case JsonToken.String: {
                                    string value = currentToken.Value.ToString();
                                    WriteValue(value, JsonValueType.STRING);
                                    break;
                                }
                            //handle a JSON integer value
                            case JsonToken.Integer: {
                                    int value = Convert.ToInt32(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.INTEGER);
                                    break;
                                }
                            //handle a JSON float value
                            case JsonToken.Float: {
                                    decimal value = Convert.ToDecimal(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.DECIMAL);
                                    break;
                                }
                            //handle a JSON boolean value
                            case JsonToken.Boolean: {
                                    bool value = Convert.ToBoolean(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.BOOLEAN);
                                    break;
                                }
                            //handle a JSON date value
                            case JsonToken.Date: {
                                    DateTime value = DateTime.Parse(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.DATE);
                                    break;
                                }
                            //handle a JSON bytes value
                            case JsonToken.Bytes: {
                                    byte[] value = (byte[])(currentToken.Value);
                                    WriteValue(value, JsonValueType.BYTES);
                                    break;
                                }
                            //handle a JSON null value
                            case JsonToken.Null: {
                                    WriteValue("", JsonValueType.NULL);
                                    break;
                                }
                            //handle "}"
                            case JsonToken.EndObject: {
                                    //output a <?object-end?> processing instruction
                                    xwriter.WriteProcessingInstruction($"object-end", null);

                                    //conditionally write an end element
                                    //and pop relevant items from stacks
                                    if (lineage.Count > 1) {
                                        xwriter.WriteEndElement();
                                        lineage.Pop();
                                        if (lineage.Peek().Type != JsonContextType.ARRAY && tags.Count > 0)
                                            tags.Pop();
                                    }

                                    break;
                                }
                            //handle "]"
                            case JsonToken.EndArray: {
                                    //output a <?array-end __________?> processing instruction
                                    xwriter.WriteProcessingInstruction($"array-end", $"{arrays.Pop().GetHashCode()}");

                                    //conditionally pop relevant items from stacks
                                    if (lineage.Count > 0)
                                        lineage.Pop();
                                    if (tags.Count > 0)
                                        tags.Pop();
                                    break;
                                }
                            default:
                                break;
                        }

                    }

                    //write the end element for the root
                    xwriter.WriteEndDocument();
                }
            }

            // return the resulting XmlDocument
            return doc;
        }


        /// <summary>
        /// Handles writing of individual values of any type,
        /// other than objects
        /// </summary>
        /// <typeparam name="T">The type of the value to write</typeparam>
        /// <param name="value">The value to write </param>
        /// <param name="jsonValueType">A string representation of the value type</param>
        private void WriteValue<T>(T value, string jsonValueType) {

            //if this value has no key, push a VALUE tag to the tags stack
            if (tags.Count == 0)
                tags.Push(new Tag { Prefix = NAMESPACE_PREFIX, Name = VALUE, URI = NAMESPACE_URI });

            //write a start element for the value
            xwriter.WriteStartElement(tags.Peek().Prefix, tags.Peek().Name, tags.Peek().URI);
            //write the value type to a jx:type attribute
            xwriter.WriteAttributeString(NAMESPACE_PREFIX, TYPE, NAMESPACE_URI, jsonValueType);

            //if this value is part of an array ... 
            if (lineage.Peek().Type == JsonContextType.ARRAY) {
                //if this is a subsequent item in the same array, output a jx:sub-sib="true" attribute
                if (lineage.Peek().ChildCount > 1)
                    xwriter.WriteAttributeString(NAMESPACE_PREFIX, SUBSIB, NAMESPACE_URI, "true");
                //output all relevant <?array ___________?> processing instructions
                WriteArrayItems();
                //increment the counter for children of this array
                lineage.Peek().ChildCount++;
            }

            //write the value
            xwriter.WriteValue(value);
            //write an end element
            xwriter.WriteEndElement();

            //if this isn't an array, pop the current tag from the tags stack
            if (lineage.Peek().Type != JsonContextType.ARRAY)
                tags.Pop();
        }


        /// <summary>
        /// Handles writing the <?array-item ____________?> processing instruction.
        /// The method has been written to handle any depth of nested arrays.
        /// </summary>
        private void WriteArrayItems() {

            //define temporary stacks for iteration use
            Stack<Parent> tmpLineage = new Stack<Parent>();
            Stack<int> tmpArrays = new Stack<int>();

            //iterate over the lineage and arrays stack until
            //the parent object is no longer an array or the 
            //array has more than one item
            while (lineage.Peek().Type == JsonContextType.ARRAY) {
                tmpLineage.Push(lineage.Pop());
                tmpArrays.Push(arrays.Pop());
                if (tmpLineage.Peek().ChildCount != 1)
                    break;
            }

            //write each relevant <?array-item _______?> processing instruction
            //and restore the lineage and array stacks
            while (tmpLineage.Count > 0) {
                xwriter.WriteProcessingInstruction($"array-item", $"{tmpArrays.Peek().GetHashCode()}");
                lineage.Push(tmpLineage.Pop());
                arrays.Push(tmpArrays.Pop());
            }

        }

    }

}
