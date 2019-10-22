using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EDennis.NetCoreTestingUtilities.Json {

    /// <summary>
    /// This class provides a method for converting JXML to
    /// JSON.  JXML is valid XML with additional markup
    /// (in the form of processing instructions).  The 
    /// additional markup allows conversion to/from JSON
    /// without any structural changes or loss of information
    /// (e.g., data types).  The markup also makes conversion
    /// to JSON much easier.  Everything is accomplished in 
    /// a single pass, with limited temporary variables.  The
    /// class makes heavy use of Newtonsoft libraries.
    /// </summary>
    public class JxmlToJson {

        /// <summary>
        /// Converts JXML to JSON
        /// </summary>
        /// <param name="doc">An XmlDocument with special processing
        /// instructions</param>
        /// <returns></returns>
        public JToken ConvertToJson(XmlDocument doc) {

            //instantiate stream-handling objects for strings
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            //initialize local variables for holding data from the JsonTextWriter
            string jsonValueType = null;
            HashSet<int> arrays = new HashSet<int>();

            //initialize a JsonTextWriter and an XmlReader
            using (JsonWriter jwriter = new JsonTextWriter(sw)) {
                using (XmlReader xreader = XmlReader.Create(new StringReader(doc.InnerXml))) {

                    //read all XML tokens, one by one
                    while (xreader.Read()) {

                        //handle the token differently, depending upon its type
                        switch (xreader.NodeType) {

                            //ignore the document type
                            case XmlNodeType.Document:
                                break;
                            //if the token is an XML element, but it is not a
                            //special jx:root, jx:object, jx:value element and
                            //it does not have a jx:ignore attribute, then 
                            //write a property key
                            case XmlNodeType.Element: {
                                    if (xreader.Prefix != JsonToJxml.NAMESPACE_PREFIX
                                        && xreader.GetAttribute(JsonToJxml.SUBSIB, JsonToJxml.NAMESPACE_URI) != "true") {
                                        jwriter.WritePropertyName(xreader.Name);
                                    }
                                    //get the data type from the jx:type attribute, if present.
                                    jsonValueType = xreader.GetAttribute(JsonToJxml.TYPE, JsonToJxml.NAMESPACE_URI);
                                    break;
                                }
                            //if it is a text element, write the appropriate value
                            case XmlNodeType.Text: {
                                    switch (jsonValueType) {
                                        case JsonValueType.BOOLEAN:
                                            jwriter.WriteValue(Convert.ToBoolean(xreader.Value.ToLower()));
                                            break;
                                        case JsonValueType.BYTES:
                                            jwriter.WriteValue(Encoding.ASCII.GetBytes(xreader.Value));
                                            break;
                                        case JsonValueType.DATE:
                                            jwriter.WriteValue(Convert.ToDateTime(xreader.Value));
                                            break;
                                        case JsonValueType.DECIMAL:
                                            jwriter.WriteValue(Convert.ToDecimal(xreader.Value));
                                            break;
                                        case JsonValueType.INTEGER:
                                            jwriter.WriteValue(Convert.ToInt32(xreader.Value));
                                            break;
                                        case JsonValueType.STRING:
                                            jwriter.WriteValue(xreader.Value);
                                            break;
                                        default:
                                            jwriter.WriteValue((bool?)null);
                                            break;
                                    }
                                    break;
                                }
                            //ignore end elements
                            case XmlNodeType.EndElement: {
                                    break;
                                }
                            //handle processing instructions
                            case XmlNodeType.ProcessingInstruction: {
                                    //if <?object-start?>, output "{"
                                    if (xreader.Name == "object-start")
                                        jwriter.WriteStartObject();
                                    //if <?object-end?>, output "}"
                                    else if (xreader.Name == "object-end")
                                        jwriter.WriteEndObject();
                                    //if <?array-item _________?> and it is
                                    //the first instance of this processing
                                    //instruction for the current array, then
                                    //output "["
                                    else if (xreader.Name == "array-item") {
                                        int id = Convert.ToInt32(xreader.Value);
                                        if (!arrays.Contains(id))
                                            jwriter.WriteStartArray();
                                        arrays.Add(id);
                                    //if <?array-end _________?> and this
                                    //array has at least one <?array-item ______?>
                                    //processing instruction, output "]".
                                    //Special Note: during XSLT processes,
                                    //<?array-end ____?> processing instructions
                                    //can be orphaned (no corresponding 
                                    //<?array-start _______?>)
                                    } else if (xreader.Name == "array-end") {
                                        int id = Convert.ToInt32(xreader.Value);
                                        if (arrays.Contains(id))
                                            jwriter.WriteEndArray();
                                        arrays.Remove(id);
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }

                }
            }
            //return a Json.NET JToken from the string
            return JToken.Parse(sb.ToString());
        }

    }

}
