// Amazon.cs
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Scripting.Actions;
using System.Text;
using System.Xml.Linq;
using DynamicRest;

namespace Services {

    public sealed class Amazon : RestClient {

        private const string AwsUrlFormat = "http://webservices.amazon.com/onca/xml?Service=AWSECommerceService&SubscriptionId={0}&Operation={1}";

        private string _subscriptionID;

        public Amazon(string subscriptionID) {
            _subscriptionID = subscriptionID;
        }

        public string SubscriptionID {
            get {
                return _subscriptionID;
            }
        }

        protected override Uri CreateRequestUri(string operation, JsonObject parameters) {
            StringBuilder uriBuilder = new StringBuilder();
            uriBuilder.AppendFormat(AwsUrlFormat, _subscriptionID, operation);

            foreach (KeyValuePair<string, object> param in (IDictionary<string, object>)parameters) {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", param.Key, param.Value);
            }

            return new Uri(uriBuilder.ToString(), UriKind.Absolute);
        }

        protected override object ProcessResponse(Stream responseStream) {
            dynamic result = null;

            try {
                string xmlText = (new StreamReader(responseStream)).ReadToEnd();
                xmlText = xmlText.Replace("xmlns=\"http://webservices.amazon.com/AWSECommerceService/2005-10-05\"", "");

                XDocument xmlDocument = XDocument.Parse(xmlText);
                return new XmlNode(xmlDocument.Root);
            }
            catch {
            }

            return result;
        }
    }
}
