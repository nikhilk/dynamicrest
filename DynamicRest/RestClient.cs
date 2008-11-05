// RestClient.cs
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Scripting.Actions;

namespace DynamicRest {

    public abstract class RestClient : DynamicObject {

        protected RestClient()
            : base(StandardActionKinds.Call) {
        }

        protected RestClient(StandardActionKinds supportedActions)
            : base(supportedActions | StandardActionKinds.Call) {
        }

        protected override object Call(CallAction action, params object[] args) {
            string operation = action.Name;

            if (IsValidRestOperation(operation)) {
                if (args.Length != 1) {
                    throw new ArgumentException("Mismatch in the number of arguments.");
                }

                JsonObject argsObject = (JsonObject)args[0];
                Uri requestUri = CreateRequestUri(operation, argsObject);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

                if (webResponse.StatusCode == HttpStatusCode.OK) {
                    Stream responseStream = webResponse.GetResponseStream();

                    return ProcessResponse(responseStream);
                }
                else {
                    return null;
                }
            }

            return base.Call(action, args);
        }

        protected abstract Uri CreateRequestUri(string operation, JsonObject parameters);

        protected abstract object ProcessResponse(Stream responseStream);

        protected virtual bool IsValidRestOperation(string name) {
            return true;
        }
    }
}
