using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.JsonUtils {

    /// <summary>
    /// Any class decorated with this attribute
    /// will be serialized as a single property
    /// value within the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonSimpleValueAttribute : Attribute {

        private string valueProperty;

        public JsonSimpleValueAttribute(string valueProperty) {
            this.valueProperty = valueProperty;
        }

        public virtual string ValueProperty {
            get { return valueProperty; }
        }

    }
}
