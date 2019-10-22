using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

namespace EDennis.NetCoreTestingUtilities.Json
{

    /// <summary>
    /// This class provides a static method for filtering JSON by 
    /// one or more JSON Path expressions
    /// <see href="http://goessner.net/articles/JsonPath/"/>
    /// </summary>
    public class JsonFilterer {

        /// <summary>
        /// Removes all pathsToIgnore from the provided JToken
        /// </summary>
        /// <param name="jToken">Valid Json.NET JToken object</param>
        /// <param name="pathsToRemove">an array of valid JSON path expressions.
        /// Note: currently, the JSON Path union operator is supported for
        /// two indexes or property names</param>
        /// <returns>JToken with the ignored paths removed</returns>
        public static JToken ApplyFilter(JToken jToken, string[] pathsToRemove) {

            //convert the JSON to XML
            JsonToJxml jx = new JsonToJxml();
            XmlDocument doc = jx.ConvertToJxml(JToken.Parse(jToken.ToString()));

            //NOTE: Json.NET deserializer does not preserve data types
            //XmlDocument doc = JsonConvert.DeserializeXmlNode(jToken.ToString(), "Root");

            //get XPath equivalents for each JSON Path expression
            for (int i = 0; i < pathsToRemove.Length; i++)
                pathsToRemove[i] = XPathFromJPath(pathsToRemove[i]);

            //apply the filter
            doc = ApplyFilter(doc, pathsToRemove);

            //convert the XML back to JSON
            JxmlToJson xj = new JxmlToJson();
            JToken result = xj.ConvertToJson(doc);

            //NOTE: Json.NET serializer does not restore data types
            //JToken result = JsonConvert.SerializeXmlNode(doc.DocumentElement, Newtonsoft.Json.Formatting.Indented, true);
            return result;
        }

        /// <summary>
        /// Converts a JSON Path expression to an XPath expression.
        /// Note: this method also accepts plain field names and xpath union
        /// of field names
        /// Special Note: all goessner.net examples convert correctly; however,
        /// the only union operations supported are index pairs and property pairs.
        /// <see href="http://goessner.net/articles/JsonPath/"/>
        /// </summary>
        /// <param name="jpath">Valid JSON Path.  
        /// NOTE: forward slashes and backslashes can be used instead of dots.
        /// Also, bare field names can be used.</param>
        /// <returns></returns>
        public static string XPathFromJPath(string jpath) {
            string xpath = jpath;

            //bypass conversion handling if a simple field name or xpath union
            if (Regex.IsMatch(jpath, "^[A-Za-z0-9_|]+$"))
                return jpath;

            //bypass conversion handling if a simple field name or xpath union of field names
            if (Regex.IsMatch(jpath, "^[A-Za-z0-9_|]+$"))
                return jpath;

            //replace forward slashes and backslashes with dots
            xpath = xpath.Replace(@"/", ".").Replace(@"\", ".");

            //escape disallowed XML characters
            //xpath = Escape(xpath);


            //handle the root element
            xpath = xpath.Replace("$.", $"/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.ROOT}/");
            //handle union operator with indexes -- only two are supported
            xpath = RegExReplace(xpath, @"\[([0-9]+),([0-9]+)\]", @"[$1 + 1 or $2 + 1]");
            //handle union operator with properties -- only two are supported
            xpath = RegExReplace(xpath, @"\[([A-Za-z0-9_]+),([A-Za-z0-9_]+)\]", @"[self::$1 or self::$2]");
            //handle regular indexes
            xpath = RegExReplace(xpath, @"\[([0-9])+\]", @"[$1 + 1]");
            //handle sliced index with open upper range
            xpath = RegExReplace(xpath, @"\[([0-9]+):\]", @"[position() > $1 + 1]");
            //handle sliced index with open lower range
            xpath = RegExReplace(xpath, @"\[:([0-9]+)\]", @"[position() < $1 + 1]");
            //handle special filter expression
            xpath = RegExReplace(xpath, @"\[\?\(\@\.([^)]+)\)\]", @"[$1]");
            //handle element value filter expression
            xpath = RegExReplace(xpath, @"\[\?\(@property\s*\=+\s*'([A-Za-z0-9_]+)'\s*&amp;&amp;\s*@([^\]]*)\)\]", @"[name() = '$1' and @ $2]");
            //handle all array elements
            xpath = xpath.Replace(@"[*]", @"[position()>0]");
            //handle last array element
            xpath = xpath.Replace(@"[-1:]", @"[last()]");
            //handle last array element
            xpath = xpath.Replace(@"(@.length-1)", @"last()");

            //replace decimal points in predicates with a guid
            var guid = Guid.NewGuid().ToString();
            xpath = RegExReplace(xpath, @"\.[0-9]", guid);

            //replace double-equals with equals
            xpath = xpath.Replace(@"==", @"=");
            //replace || with or
            xpath = xpath.Replace(@"||", @" or ");
            //replace && with or
            xpath = xpath.Replace(@"&amp;&amp;", @" and ");
            //replace double-quote with single-quote
            xpath = xpath.Replace("\"", "'");


            //replace dot with forward slash for path separator
            xpath = xpath.Replace(@".", @"/");
            //replace @ with . for ::self node
            xpath = xpath.Replace(@"@", @".");

            //restore decimal points in predicates
            xpath = xpath.Replace(guid, ".");

            //handle arrays of objects, whose XPATH begins with /jx:root
            if (xpath.StartsWith("/jx:root")){
                xpath = xpath + "|" + xpath.Replace($"/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.ROOT}/",
                    $"/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.ROOT}/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.OBJECT}")

                    + "|" + xpath.Replace($"/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.ROOT}/",
                    $"/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.ROOT}/{JsonToJxml.NAMESPACE_PREFIX + ":" + JsonToJxml.VALUE}");
            }
            xpath = xpath.Replace("/[", "[").Replace("/[", "["); //handle invalid path
            return xpath;
        }


        /// <summary>
        /// Performs regular expression replacements
        /// </summary>
        /// <param name="input">The source string</param>
        /// <param name="findPattern">The regular expression pattern to find</param>
        /// <param name="replacePattern">The regular expression replacement pattern</param>
        /// <returns>A new string reflecting the replacement</returns>
        private static string RegExReplace(string input, string findPattern, string replacePattern) {
            var regex = new Regex(findPattern);
            return regex.Replace(input, replacePattern);
        }


        /// <summary>
        /// Removes all XML paths from an XML document
        /// </summary>
        /// <param name="document">The source XML document</param>
        /// <param name="pathsToRemove">An array of XPaths to remove</param>
        /// <returns>A new XML document with all target paths removed</returns>
        private static XmlDocument ApplyFilter(XmlDocument document, string[] pathsToRemove) {
            //document.Save("F:\\" + Guid.NewGuid().ToString() + ".xml");
            //get the XSLT for the transformation
            string xslt = GetFilterXslt(pathsToRemove);
            //File.WriteAllText("F:\\" + Guid.NewGuid().ToString() + ".xslt",xslt);
            //instantiate the document to return
            var doc = new XmlDocument();

            //transform the document
            using (var xsltReader = XmlReader.Create(new StringReader(xslt))) {
                using (var writer = new StringWriter()) {

                    var myXslTrans = new XslCompiledTransform();
                    myXslTrans.Load(xsltReader);
                    myXslTrans.Transform(document.CreateNavigator(), null, writer);

                    doc.LoadXml(writer.ToString());
                }
            }

           // doc.Save("E:\\" + Guid.NewGuid().ToString() + ".xml");

            //return the transformed document
            return doc;
        }


        /// <summary>
        /// Builds XSLT that removes all desired XPaths
        /// </summary>
        /// <param name="pathsToRemove">An array of XPaths to remove</param>
        /// <returns>XSLT for removing the XPaths</returns>
        private static string GetFilterXslt(string[] pathsToRemove) {

            var xslt = new StringBuilder();
            xslt.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xslt.Append(" <xsl:stylesheet version=\"1.0\"");
            xslt.Append(" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" ");
            xslt.Append(" xmlns:" + JsonToJxml.NAMESPACE_PREFIX);
            xslt.Append("=\"" + JsonToJxml.NAMESPACE_URI + "\"");
            xslt.Append(">");
            xslt.Append("<xsl:template match=\"@* | node() | processing-instruction()\">");
            xslt.Append("<xsl:copy>");
            xslt.Append("<xsl:apply-templates select=\"@* | node() | processing-instruction()\" />");
            xslt.Append("</xsl:copy>");
            xslt.Append("</xsl:template>");

            //build the part of the XSLT that removes the XPaths
            foreach (string pathToRemove in pathsToRemove) {
                xslt.Append("<xsl:template match=\"" + pathToRemove + "\" />");
            }

            xslt.Append("</xsl:stylesheet>");
            return xslt.ToString();
        }

        /// <summary>
        /// Replaces all instances of invalid XML characters with their entity equivalents
        /// </summary>
        /// <param name="input">The source string</param>
        /// <returns>A new string with all invalid characters escaped</returns>
        private static string Escape(string input) {
            string str = input.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            return str;
        }


    }
}
