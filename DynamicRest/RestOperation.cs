// RestOperation.cs
//

using System;
using System.Net;

namespace DynamicRest {

    public sealed class RestOperation {

        private object _result;
        private Exception _error;

        private HttpStatusCode _statusCode;
        private string _statusMessage;
        private bool _completed;

        internal RestOperation() {
        }

        public Exception Error {
            get {
                return _error;
            }
        }

        public bool IsCompleted {
            get {
                return _completed;
            }
        }

        public dynamic Result {
            get {
                return _result;
            }
        }

        public HttpStatusCode StatusCode {
            get {
                return _statusCode;
            }
        }

        public string StatusMessage {
            get {
                return _statusMessage;
            }
        }

        internal void Complete(object result, HttpStatusCode statusCode, string statusMessage) {
            Complete(result, null, statusCode, statusMessage);
        }

        internal void Complete(Exception error, HttpStatusCode statusCode, string statusMessage) {
            Complete(null, error, statusCode, statusMessage);
        }

        private void Complete(object result, Exception error, HttpStatusCode statusCode, string statusMessage) {
            _result = result;
            _error = error;
            _statusCode = statusCode;
            _statusMessage = statusMessage;
            _completed = true;
        }
    }
}
