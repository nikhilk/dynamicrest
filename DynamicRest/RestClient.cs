// RestClient.cs
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Scripting.Actions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace DynamicRest {

    public sealed class RestClient : DynamicObject {

        private static readonly Regex TokenFormatRewriteRegex =
            new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                       RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant); 

        private static readonly Regex StripXmlnsRegex =
            new Regex(@"(xmlns:?[^=]*=[""][^""]*[""])",
                      RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private string _uriFormat;
        private RestClientMode _mode;
        private string _operationGroup;
        private Dictionary<string, object> _parameters;

        public RestClient(string uriFormat, RestClientMode mode)
            : base(StandardActionKinds.GetMember | StandardActionKinds.SetMember |
                   StandardActionKinds.Call | StandardActionKinds.Invoke) {
            _uriFormat = uriFormat;
            _mode = mode;
        }

        private RestClient(string uriFormat, RestClientMode mode, string operationGroup, Dictionary<string, object> inheritedParameters)
            : this(uriFormat, mode) {
            _operationGroup = operationGroup;
            _parameters = inheritedParameters;
        }

        protected override object Call(CallAction action, params object[] args) {
            string operation = action.Name;
            if (_operationGroup != null) {
                operation = _operationGroup + "." + operation;
            }

            return PerformOperation(operation, args);
        }

        private Uri CreateRequestUri(string operation, JsonObject parameters) {
            StringBuilder uriBuilder = new StringBuilder();

            List<object> values = new List<object>();

            string rewrittenUriFormat = TokenFormatRewriteRegex.Replace(_uriFormat, delegate(Match m) {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                if ((operation.Length != 0) && String.CompareOrdinal(propertyGroup.Value, "operation") == 0) {
                    values.Add(operation);
                }
                else if (_parameters != null) {
                    values.Add(_parameters[propertyGroup.Value]);
                }

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value + new string('}', endGroup.Captures.Count);
            });

            if (values.Count != 0) {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture, rewrittenUriFormat, values.ToArray()); 
            }
            else {
                uriBuilder.Append(rewrittenUriFormat);
            }
            if (rewrittenUriFormat.IndexOf('?') < 0) {
                uriBuilder.Append("?");
            }

            if (parameters != null) {
                foreach (KeyValuePair<string, object> param in (IDictionary<string, object>)parameters) {
                    string value = String.Format(CultureInfo.InvariantCulture, "{0}", param.Value);
                    value = HttpUtility.UrlEncode(value);

                    uriBuilder.AppendFormat("&{0}={1}", param.Key, value);
                }
            }

            return new Uri(uriBuilder.ToString(), UriKind.Absolute);
        }

        protected override object GetMember(GetMemberAction action) {
            if (_parameters == null) {
                _parameters = new Dictionary<string, object>();
            }

            object value;
            if (_parameters.TryGetValue(action.Name, out value)) {
                return value;
            }

            string operationGroup = action.Name;
            if (_operationGroup != null) {
                operationGroup = _operationGroup + "." + operationGroup;
            }

            RestClient operationGroupClient = new RestClient(_uriFormat, _mode, operationGroup, _parameters);
            return operationGroupClient;
        }

        protected override object Invoke(InvokeAction action, params object[] args) {
            return PerformOperation(String.Empty, args);
        }

        private object PerformOperation(string operation, params object[] args) {
            JsonObject argsObject = null;
            if ((args != null) && (args.Length != 0)) {
                argsObject = (JsonObject)args[0];
            }
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

        private object ProcessResponse(Stream responseStream) {
            dynamic result = null;

            try {
                string responseText = (new StreamReader(responseStream)).ReadToEnd();
                if (_mode == RestClientMode.Json) {
                    JsonReader jsonReader = new JsonReader(responseText);
                    result = jsonReader.ReadValue();
                }
                else {
                    responseText = StripXmlnsRegex.Replace(responseText, String.Empty);
                    XDocument xmlDocument = XDocument.Parse(responseText);

                    result = new XmlNode(xmlDocument.Root);
                }
            }
            catch {
            }

            return result;
        }

        protected override void SetMember(SetMemberAction action, object value) {
            if (_parameters == null) {
                _parameters = new Dictionary<string, object>();
            }
            _parameters[action.Name] = value;
        }
    }
}
