using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace EDennis.NetCoreTestingUtilities {


    /// <summary>
    /// Sorts Json Array elements by value and properties by name
    /// </summary>
    public class JsonSorter {
        /// <summary>
        /// Deeply sorts all nested arrays and objects in a JSON string
        /// </summary>
        /// <param name="json">A string represnting valid JSON</param>
        /// <returns>A sorted version of the JSON</returns>
        public static string Sort(string json) {
            JToken jtoken = null;
            JToken sortedJtoken = null;
            try {
                jtoken = JToken.Parse(json);
            } catch (Exception ex) {
                throw new ApplicationException($"{ex.Message}: Cannot parse json string: {json}");
            }
            try {
                sortedJtoken = Sort(jtoken);
            } catch (Exception ex) {
                throw new ApplicationException($"{ex.Message}: Cannot sort json string: {json}");
            }
            return sortedJtoken.ToString();
        }

        /// <summary>
        /// Recursively sorts a JSON.NET JToken
        /// </summary>
        /// <param name="jtoken">The Internal tokenized representation of the JSON</param>
        /// <returns>a recursively sorted JToken object</returns>
        public static JToken Sort(JToken jtoken) {

            //base case 1: not a container
            if (!(jtoken is JContainer))
                return jtoken;

            bool childIsContainer = jtoken.ChildIsContainer();
            var childType = jtoken.ChildType();

            //base case 2: property and child is not a container
            if (jtoken is JProperty && !childIsContainer) {
                return jtoken;
            }

            //treat jtoken as container via jcontainer variable
            var jcontainer = jtoken as JContainer;

            //initialize a list of strings for storing serialized child tokens
            var serializedElements = new SortedDictionary<string, JToken>();

            //loop through all child tokens of the current token
            var children = jcontainer.Children();
            foreach (var child in children) {
                //recursively sort the child token
                var sorted = Sort(child);
                //serialize the sorted child token and add it to the string list
                serializedElements.Add(sorted.ToString(), child);
            }

            //handle JProperty
            if (jcontainer is JProperty && childIsContainer) {
                JContainer propValue = null;
                if (childType == JTokenType.Array)
                    propValue = new JArray();
                else
                    propValue = new JObject();

                foreach (var key in serializedElements.Keys) {
                    var sortedChildToAdd = serializedElements[key];
                    if (sortedChildToAdd.Type == JTokenType.Object && propValue.Type == JTokenType.Object)
                        propValue.Add(new JProperty((jcontainer as JProperty).Name, propValue));
                    else
                        propValue.Add(sortedChildToAdd);
                }

                jcontainer = new JProperty((jcontainer as JProperty).Name, propValue);

                //handle JObject and JArray
            } else {
                jcontainer.RemoveAll();

                foreach (var key in serializedElements.Keys)
                    jcontainer.Add(serializedElements[key]);
            }

            return jcontainer;
        }
    }


    /// <summary>
    /// Provides a convenient extension method for returning the token type
    /// of a given JToken object (or Null if the object has no children)
    /// </summary>
    public static class JsonExtensions {
        public static JTokenType ChildType(this JToken obj) {
            var children = obj.Children();
            foreach (var child in children) {
                return child.Type;
            }
            return JTokenType.Null;
        }

        public static bool ChildIsContainer(this JToken obj) {
            var children = obj.Children();
            foreach (var child in children) {
                if (child is JContainer)
                    return true;
            }
            return false;
        }
    }

}

