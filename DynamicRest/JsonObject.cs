// JsonObject.cs
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Scripting.Actions;

namespace DynamicRest {

    public sealed class JsonObject : DynamicObject, IDictionary<string, object>, IDictionary {

        private Dictionary<string, object> _members;

        public JsonObject()
            : base(StandardActionKinds.GetMember | StandardActionKinds.SetMember | StandardActionKinds.DeleteMember |
                   StandardActionKinds.Convert) {
            _members = new Dictionary<string, object>();
        }

        public JsonObject(params object[] nameValuePairs)
            : this() {
            if (nameValuePairs != null) {
                if (nameValuePairs.Length % 2 != 0) {
                    throw new ArgumentException("Mismatch in name/value pairs.");
                }

                for (int i = 0; i < nameValuePairs.Length; i += 2) {
                    if (!(nameValuePairs[i] is string)) {
                        throw new ArgumentException("Name parameters must be strings.");
                    }

                    _members[(string)nameValuePairs[i]] = nameValuePairs[i + 1];
                }
            }
        }

        protected override object Convert(ConvertAction action) {
            Type targetType = action.ToType;

            if ((targetType == typeof(IEnumerable)) ||
                (targetType == typeof(IEnumerable<KeyValuePair<string, object>>)) ||
                (targetType == typeof(IDictionary<string, object>)) ||
                (targetType == typeof(IDictionary))) {
                return this;
            }

            return base.Convert(action);
        }

        protected override bool DeleteMember(DeleteMemberAction action) {
            return _members.Remove(action.Name);
        }

        protected override object GetMember(GetMemberAction action) {
            object value;
            if (_members.TryGetValue(action.Name, out value)) {
                return value;
            }
            return base.GetMember(action);
        }

        protected override void SetMember(SetMemberAction action, object value) {
            _members[action.Name] = value;
        }

        #region Implementation of IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return new DictionaryEnumerator(_members.GetEnumerator());
        }
        #endregion

        #region Implementation of IEnumerable<KeyValuePair<string, object>>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() {
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

        #region Implementation of ICollection<KeyValuePair<string, object>>
        int ICollection<KeyValuePair<string, object>>.Count {
            get {
                return _members.Count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get {
                return false;
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) {
            ((IDictionary<string, object>)_members).Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear() {
            _members.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) {
            return ((IDictionary<string, object>)_members).Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            ((IDictionary<string, object>)_members).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) {
            return ((IDictionary<string, object>)_members).Remove(item);
        }
        #endregion

        #region Implementation of IDictionary
        bool IDictionary.IsFixedSize {
            get {
                return false;
            }
        }

        bool IDictionary.IsReadOnly {
            get {
                return false;
            }
        }

        ICollection IDictionary.Keys {
            get {
                return _members.Keys;
            }
        }

        object IDictionary.this[object key] {
            get {
                return _members[(string)key];
            }
            set {
                _members[(string)key] = value;
            }
        }

        ICollection IDictionary.Values {
            get {
                return _members.Values;
            }
        }

        void IDictionary.Add(object key, object value) {
            _members.Add((string)key, value);
        }

        void IDictionary.Clear() {
            _members.Clear();
        }

        bool IDictionary.Contains(object key) {
            return _members.ContainsKey((string)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return (IDictionaryEnumerator)((IEnumerable)this).GetEnumerator();
        }

        void IDictionary.Remove(object key) {
            _members.Remove((string)key);
        }
        #endregion

        #region Implementation of IDictionary<string, object>
        ICollection<string> IDictionary<string, object>.Keys {
            get {
                return _members.Keys;
            }
        }

        object IDictionary<string, object>.this[string key] {
            get {
                return _members[key];
            }
            set {
                _members[key] = value;
            }
        }

        ICollection<object> IDictionary<string, object>.Values {
            get {
                return _members.Values;
            }
        }

        void IDictionary<string, object>.Add(string key, object value) {
            _members.Add(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key) {
            return _members.ContainsKey(key);
        }

        bool IDictionary<string, object>.Remove(string key) {
            return _members.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value) {
            return _members.TryGetValue(key, out value);
        }
        #endregion


        private sealed class DictionaryEnumerator : IDictionaryEnumerator {

            private IEnumerator<KeyValuePair<string, object>> _enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<string, object>> enumerator) {
                _enumerator = enumerator;
            }

            public object Current {
                get {
                    return Entry;
                }
            }

            public DictionaryEntry Entry {
                get {
                    return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);
                }
            }

            public object Key {
                get {
                    return _enumerator.Current.Key;
                }
            }

            public object Value {
                get {
                    return _enumerator.Current.Value;
                }
            }

            public bool MoveNext() {
                return _enumerator.MoveNext();
            }

            public void Reset() {
                _enumerator.Reset();
            }
        }
    }
}
