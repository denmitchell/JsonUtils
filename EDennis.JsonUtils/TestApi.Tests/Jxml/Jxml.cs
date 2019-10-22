using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EDennis.NetCoreTestingUtilities.Json {

    /// <summary>
    /// This class provides static methods for converting
    /// JToken objects to XmlDocument objects and XmlDocument
    /// objects to JToken objects.  The XmlDocument conforms 
    /// to a special structure, having processing instructions
    /// and special, generic elements and attributes for
    /// ensuring that the JSON can be fully recovered from 
    /// the XML.  This special structure is called "JXML"
    /// by the author.
    /// </summary>
    public class Jxml {

        /// <summary>
        /// Converts an XmlDocument object to a 
        /// Json.NET JToken.
        /// </summary>
        /// <param name="doc">Valid XmlDocument object</param>
        /// <returns>JToken object</returns>
        public static JToken ToJson(XmlDocument doc) {
            var jx = new JxmlToJson();
            return jx.ConvertToJson(doc);
        }

        /// <summary>
        /// Converts a Json.NET JToken to an
        /// XmlDocument object
        /// </summary>
        /// <param name="jtoken">Valid Json.NET JToken</param>
        /// <returns>XmlDocument object</returns>
        public static XmlDocument ToJxml(JToken jtoken) {
            var jx = new JsonToJxml();
            return jx.ConvertToJxml(jtoken);
        }

    }
}