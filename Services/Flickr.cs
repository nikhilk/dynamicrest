// Flickr.cs
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Scripting.Actions;
using System.Text;
using DynamicRest;

namespace Services {

    public sealed class Flickr : DynamicObject {

        private string _apiKey;

        public Flickr(string apiKey)
            : base(StandardActionKinds.GetMember) {
            _apiKey = apiKey;
        }

        public string ApiKey {
            get {
                return _apiKey;
            }
        }

        protected override object GetMember(GetMemberAction action) {
            return new FlickrClient(_apiKey, action.Name.ToLower(CultureInfo.InvariantCulture));
        }


        private sealed class FlickrClient : RestClient {

            private const string FlickrUrlFormat = "http://api.flickr.com/services/rest/?method=flickr.{0}.{1}&api_key={2}&format=json&nojsoncallback=1";

            private string _apiKey;
            private string _apiGroup;

            internal FlickrClient(string apiKey, string apiGroup)
                : base(StandardActionKinds.Call) {
                _apiKey = apiKey;
                _apiGroup = apiGroup;
            }

            protected override Uri CreateRequestUri(string operation, JsonObject parameters) {
                StringBuilder uriBuilder = new StringBuilder();
                uriBuilder.AppendFormat(FlickrUrlFormat, _apiGroup, operation, _apiKey);

                foreach (KeyValuePair<string, object> param in (IDictionary<string, object>)parameters) {
                    uriBuilder.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", param.Key, param.Value);
                }

                return new Uri(uriBuilder.ToString(), UriKind.Absolute);
            }

            protected override object ProcessResponse(Stream responseStream) {
                dynamic result = null;

                try {
                    string jsonText = (new StreamReader(responseStream)).ReadToEnd();
                    JsonReader jsonReader = new JsonReader(jsonText);
                    result = jsonReader.ReadValue();
                }
                catch {
                }

                return result;
            }
        }
    }
}
