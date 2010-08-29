// XmlNodeList.cs
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Dynamic;

namespace DynamicRest {

    public sealed class XmlNodeList : DynamicObject, IEnumerable {

        private List<XElement> _elements;

        internal XmlNodeList(IEnumerable<XElement> elements)
            : base() {
            _elements = new List<XElement>(elements);
        }

        public override bool TryConvert(ConvertBinder binder, out object result) {
            Type targetType = binder.Type;
            if (targetType == typeof(IEnumerable)) {
                result = this;
                return true;
            }
            return base.TryConvert(binder, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            if (indexes.Length == 1) {
                XElement element = _elements[Convert.ToInt32(indexes[0])];
                result = new XmlNode(element);
                return true;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (String.Compare("Length", binder.Name, StringComparison.Ordinal) == 0) {
                result = _elements.Count;
                return true;
            }

            return base.TryGetMember(binder, out result);
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
