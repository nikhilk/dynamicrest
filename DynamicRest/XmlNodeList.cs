// XmlNodeList.cs
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Scripting.Actions;
using System.Xml.Linq;

namespace DynamicRest {

    public sealed class XmlNodeList : DynamicObject, IEnumerable {

        private List<XElement> _elements;

        internal XmlNodeList(IEnumerable<XElement> elements)
            : base(StandardActionKinds.GetMember | StandardActionKinds.Call |
                   StandardActionKinds.Convert) {
            _elements = new List<XElement>(elements);
        }

        protected override object Convert(ConvertAction action) {
            Type targetType = action.ToType;

            if (targetType == typeof(IEnumerable)) {
                return this;
            }

            return base.Convert(action);
        }

        protected override object Call(CallAction action, params object[] args) {
            if (String.Compare(action.Name, "Item", StringComparison.Ordinal) == 0) {
                if (args.Length == 1) {
                    XElement element = _elements[System.Convert.ToInt32(args[0])];
                    return new XmlNode(element);
                }
            }

            return base.Call(action, args);
        }

        protected override object GetMember(GetMemberAction action) {
            if (String.Compare("Length", action.Name, StringComparison.Ordinal) == 0) {
                return _elements.Count;
            }

            return base.GetMember(action);
        }

        #region Implementation of IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return new NodeEnumerator(_elements.GetEnumerator());
        }
        #endregion

        private sealed class NodeEnumerator : IEnumerator {

            private IEnumerator<XElement> _elementEnumerator;

            public NodeEnumerator(IEnumerator<XElement> elementEnumerator) {
                _elementEnumerator = elementEnumerator;
            }

            public object Current {
                get {
                    XElement element = _elementEnumerator.Current;
                    return new XmlNode(element);
                }
            }

            public bool MoveNext() {
                return _elementEnumerator.MoveNext();
            }

            public void Reset() {
                _elementEnumerator.Reset();
            }
        }
    }
}
