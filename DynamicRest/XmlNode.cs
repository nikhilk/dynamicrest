// XmlNode.cs
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Scripting.Actions;
using System.Text;
using System.Xml.Linq;

namespace DynamicRest {

    public sealed class XmlNode : DynamicObject {

        private XElement _element;

        public XmlNode(string name) : this(new XElement(name)) {
        }

        public XmlNode(XElement element)
            : base(StandardActionKinds.GetMember | StandardActionKinds.SetMember | StandardActionKinds.Call) {
            _element = element;
        }

        protected override object Call(CallAction action, params object[] args) {
            string name = action.Name;

            if (String.CompareOrdinal(name, "SelectAll") == 0) {
                IEnumerable<XElement> selectedElements = null;

                if (args.Length == 0) {
                    selectedElements = _element.Descendants();
                }
                else {
                    selectedElements = _element.Descendants(args[0].ToString());
                }

                return new XmlNodeList(selectedElements);
            }
            else if (String.CompareOrdinal(name, "SelectChildren") == 0) {
                IEnumerable<XElement> selectedElements = null;

                if (args.Length == 0) {
                    selectedElements = _element.Elements();
                }
                else {
                    selectedElements = _element.Elements(args[0].ToString());
                }

                return new XmlNodeList(selectedElements);
            }

            return base.Call(action, args);
        }

        protected override object GetMember(GetMemberAction action) {
            string name = action.Name;

            if (String.CompareOrdinal(name, "Name") == 0) {
                return _element.Name.LocalName;
            }
            else if (String.CompareOrdinal(name, "Parent") == 0) {
                XElement parent = _element.Parent;
                if (parent != null) {
                    return new XmlNode(parent);
                }
                return null;
            }
            else if (String.CompareOrdinal(name, "Value") == 0) {
                return _element.Value;
            }
            else if (String.CompareOrdinal(name, "Nodes") == 0) {
                return new XmlNodeList(_element.Elements());
            }
            else if (String.CompareOrdinal(name, "Xml") == 0) {
                StringWriter sw = new StringWriter();
                _element.Save(sw, SaveOptions.None);

                return sw.ToString();
            }
            else {
                XAttribute attribute = _element.Attribute(name);
                if (attribute != null) {
                    return attribute.Value;
                }

                XElement childNode = _element.Element(name);
                if (childNode != null) {
                    if (childNode.HasElements == false) {
                        return childNode.Value;
                    }
                    return new XmlNode(childNode);
                }
            }

            return base.GetMember(action);
        }

        protected override void SetMember(SetMemberAction action, object value) {
            string name = action.Name;

            if (String.CompareOrdinal(name, "Value") == 0) {
                _element.Value = (value != null) ? value.ToString() : String.Empty;
            }

            base.SetMember(action, value);
        }
    }
}
