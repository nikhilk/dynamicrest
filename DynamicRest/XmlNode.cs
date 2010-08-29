// XmlNode.cs
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Dynamic;

namespace DynamicRest {

    public sealed class XmlNode : DynamicObject {

        private XElement _element;

        public XmlNode(string name)
            : this(new XElement(name)) {
        }

        public XmlNode(XElement element) {
            _element = element;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            string name = binder.Name;

            if (String.CompareOrdinal(name, "Name") == 0) {
                result = _element.Name.LocalName;
                return true;
            }
            else if (String.CompareOrdinal(name, "Parent") == 0) {
                XElement parent = _element.Parent;
                if (parent != null) {
                    result = new XmlNode(parent);
                    return true;
                }
                result = null;
                return false;
            }
            else if (String.CompareOrdinal(name, "Value") == 0) {
                result = _element.Value;
                return true;
            }
            else if (String.CompareOrdinal(name, "Nodes") == 0) {
                result = new XmlNodeList(_element.Elements());
                return true;
            }
            else if (String.CompareOrdinal(name, "Xml") == 0) {
                StringWriter sw = new StringWriter();
                _element.Save(sw, SaveOptions.None);

                result = sw.ToString();
                return true;
            }
            else {
                XAttribute attribute = _element.Attribute(name);
                if (attribute != null) {
                    result = attribute.Value;
                    return true;
                }

                XElement childNode = _element.Element(name);
                if (childNode != null) {
                    if (childNode.HasElements == false) {
                        result = childNode.Value;
                        return true;
                    }
                    result = new XmlNode(childNode);
                    return true;
                }
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            string name = binder.Name;

            if (String.CompareOrdinal(name, "SelectAll") == 0) {
                IEnumerable<XElement> selectedElements = null;

                if (args.Length == 0) {
                    selectedElements = _element.Descendants();
                }
                else if (args.Length == 1) {
                    selectedElements = _element.Descendants(args[0].ToString());
                }
                else {
                    result = false;
                    return false;
                }
                result = new XmlNodeList(selectedElements);
                return true;
            }
            else if (String.CompareOrdinal(name, "SelectChildren") == 0) {
                IEnumerable<XElement> selectedElements = null;

                if (args.Length == 0) {
                    selectedElements = _element.Elements();
                }
                else if (args.Length == 1) {
                    selectedElements = _element.Elements(args[0].ToString());
                }
                else {
                    result = false;
                    return false;
                }
                result = new XmlNodeList(selectedElements);
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            string name = binder.Name;

            if (String.CompareOrdinal(name, "Value") == 0) {
                _element.Value = (value != null) ? value.ToString() : String.Empty;
                return true;
            }

            return base.TrySetMember(binder, value);
        }
    }
}
