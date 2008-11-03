// JsonArray.cs
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Scripting.Actions;

namespace DynamicRest {

    public sealed class JsonArray : DynamicObject, ICollection<object>, ICollection {

        private List<object> _members;

        public JsonArray()
            : base(StandardActionKinds.GetMember | StandardActionKinds.Call |
                   StandardActionKinds.Convert) {
            _members = new List<object>();
        }

        public JsonArray(object o)
            : this() {
            _members.Add(o);
        }

        public JsonArray(object o1, object o2)
            : this() {
            _members.Add(o1);
            _members.Add(o2);
        }

        public JsonArray(params object[] objects)
            : this() {
            _members.AddRange(objects);
        }

        protected override object Convert(ConvertAction action) {
            Type targetType = action.ToType;

            if ((targetType == typeof(IEnumerable)) ||
                (targetType == typeof(IEnumerable<object>)) ||
                (targetType == typeof(ICollection<object>)) ||
                (targetType == typeof(ICollection))) {
                return this;
            }

            return base.Convert(action);
        }

        protected override object Call(CallAction action, params object[] args) {
            if (String.Compare(action.Name, "Item", StringComparison.Ordinal) == 0) {
                if (args.Length == 1) {
                    return _members[System.Convert.ToInt32(args[0])];
                }
                else if (args.Length == 2) {
                    _members[System.Convert.ToInt32(args[0])] = args[1];
                    return null;
                }
            }
            else if (String.Compare(action.Name, "Add", StringComparison.Ordinal) == 0) {
                _members.Add(args[0]);
                return null;
            }
            else if (String.Compare(action.Name, "Insert", StringComparison.Ordinal) == 0) {
                _members.Insert(System.Convert.ToInt32(args[0]), args[1]);
                return null;
            }
            else if (String.Compare(action.Name, "IndexOf", StringComparison.Ordinal) == 0) {
                return _members.IndexOf(args[0]);
            }
            else if (String.Compare(action.Name, "Clear", StringComparison.Ordinal) == 0) {
                _members.Clear();
                return null;
            }
            else if (String.Compare(action.Name, "Remove", StringComparison.Ordinal) == 0) {
                return _members.Remove(args[0]);
            }
            else if (String.Compare(action.Name, "RemoveAt", StringComparison.Ordinal) == 0) {
                _members.RemoveAt(System.Convert.ToInt32(args[0]));
                return null;
            }

            return base.Call(action, args);
        }

        protected override object GetMember(GetMemberAction action) {
            if (String.Compare("Length", action.Name, StringComparison.Ordinal) == 0) {
                return _members.Count;
            }

            return base.GetMember(action);
        }

        #region Implementation of IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return _members.GetEnumerator();
        }
        #endregion

        #region Implementation of IEnumerable<object>
        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return _members.GetEnumerator();
        }
        #endregion

        #region Implementation of ICollection
        int ICollection.Count {
            get {
                return _members.Count;
            }
        }

        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }
        #endregion

        #region Implementation of ICollection<object>
        int ICollection<object>.Count {
            get {
                return _members.Count;
            }
        }

        bool ICollection<object>.IsReadOnly {
            get {
                return false;
            }
        }

        void ICollection<object>.Add(object item) {
            ((ICollection<object>)_members).Add(item);
        }

        void ICollection<object>.Clear() {
            ((ICollection<object>)_members).Clear();
        }

        bool ICollection<object>.Contains(object item) {
            return ((ICollection<object>)_members).Contains(item);
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex) {
            ((ICollection<object>)_members).CopyTo(array, arrayIndex);
        }

        bool ICollection<object>.Remove(object item) {
            return ((ICollection<object>)_members).Remove(item);
        }
        #endregion
    }
}
