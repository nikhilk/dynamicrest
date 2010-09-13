// RestClient.cs
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
        private RestService _service;
        private IRestUriTransformer _uriTransformer;
        private string _operationGroup;
        private Dictionary<string, object> _parameters;

        public RestClient(string uriFormat, RestService service) {
            _uriFormat = uriFormat;
            _service = service;
        }

        public RestClient(string uriFormat, RestService service, IRestUriTransformer uriTransformer)
            : this(uriFormat, service) {
            _uriTransformer = uriTransformer;
        }

        private RestClient(string uriFormat, RestService service, string operationGroup, Dictionary<string, object> inheritedParameters)
            : this(uriFormat, service) {
            _operationGroup = operationGroup;
            _parameters = inheritedParameters;
        }

        private Uri CreateRequestUri(string operationName, JsonObject parameters) {
            StringBuilder uriBuilder = new StringBuilder();

            List<object> values = new List<object>();
            HashSet<string> addedParameters = null;

            string rewrittenUriFormat = TokenFormatRewriteRegex.Replace(_uriFormat, delegate(Match m) {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                if ((operationName.Length != 0) && String.CompareOrdinal(propertyGroup.Value, "operation") == 0) {
                    values.Add(operationName);
                }
                else if (_parameters != null) {
                    values.Add(_parameters[propertyGroup.Value]);

                    if (addedParameters == null) {
                        addedParameters = new HashSet<string>(StringComparer.Ordinal);
                    }
                    addedParameters.Add(propertyGroup.Value);
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
                    string name = param.Key;
                    object value = param.Value;

                    if ((addedParameters != null) && addedParameters.Contains(name)) {
                        // Already consumed above to substitute a named token
                        // in the URI format.
                        continue;
                    }

                    if (value is Delegate) {
                        // Ignore callbacks in the async scenario.
                        continue;
                    }

                    if (value is JsonObject) {
                        // Nested object... use name.subName=value format.

                        foreach (KeyValuePair<string, object> nestedParam in (IDictionary<string, object>)value) {
                            uriBuilder.AppendFormat("&{0}.{1}={2}",
                                                    name, nestedParam.Key,
                                                    FormatUriParameter(nestedParam.Value));
                        }

                        continue;
                    }

                    uriBuilder.AppendFormat("&{0}={1}", name, FormatUriParameter(value));
                }
            }

            Uri uri = new Uri(uriBuilder.ToString(), UriKind.Absolute);
            if (_uriTransformer != null) {
                uri = _uriTransformer.TransformUri(uri);
            }

            return uri;
        }

        private string FormatUriParameter(object value) {
            if (value is IEnumerable<string>) {
                return String.Join("+", (IEnumerable<string>)value);
            }
            else {
                return HttpUtility.UrlEncode(String.Format(CultureInfo.InvariantCulture, "{0}", value));
            }
        }

        private RestOperation PerformOperation(string operationName, params object[] args) {
            JsonObject argsObject = null;
            if ((args != null) && (args.Length != 0)) {
                argsObject = (JsonObject)args[0];
            }

            RestOperation operation = new RestOperation();

            Uri requestUri = CreateRequestUri(operationName, argsObject);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            if (webResponse.StatusCode == HttpStatusCode.OK) {
                Stream responseStream = webResponse.GetResponseStream();

                try {
                    object result = ProcessResponse(responseStream);
                    operation.Complete(result,
                                       webResponse.StatusCode, webResponse.StatusDescription);
                }
                catch (Exception e) {
                    operation.Complete(new WebException(e.Message, e),
                                       webResponse.StatusCode, webResponse.StatusDescription);
                }
            }
            else {
                operation.Complete(new WebException(webResponse.StatusDescription),
                                   webResponse.StatusCode, webResponse.StatusDescription);
            }

            return operation;
        }

        private RestOperation PerformOperationAsync(string operationName, params object[] args) {
            JsonObject argsObject = null;
            if ((args != null) && (args.Length != 0)) {
                argsObject = (JsonObject)args[0];
            }

            RestOperation operation = new RestOperation();

            Uri requestUri = CreateRequestUri(operationName, argsObject);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUri);

            webRequest.BeginGetResponse((ar) => {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.EndGetResponse(ar);
                if (webResponse.StatusCode == HttpStatusCode.OK) {
                    Stream responseStream = webResponse.GetResponseStream();

                    try {
                        object result = ProcessResponse(responseStream);
                        operation.Complete(result,
                                           webResponse.StatusCode, webResponse.StatusDescription);
                    }
                    catch (Exception e) {
                        operation.Complete(new WebException(e.Message, e),
                                           webResponse.StatusCode, webResponse.StatusDescription);
                    }
                }
                else {
                    operation.Complete(new WebException(webResponse.StatusDescription),
                                       webResponse.StatusCode, webResponse.StatusDescription);
                }
            }, null);

            return operation;
        }

        private object ProcessResponse(Stream responseStream) {
            dynamic result = null;

            try {
                string responseText = (new StreamReader(responseStream)).ReadToEnd();
                if (_service == RestService.Json) {
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

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (_parameters == null) {
                _parameters = new Dictionary<string, object>();
            }

            object value;
            if (_parameters.TryGetValue(binder.Name, out value)) {
                result = value;
                return true;
            }

            string operationGroup = binder.Name;
            if (_operationGroup != null) {
                operationGroup = _operationGroup + "." + operationGroup;
            }

            RestClient operationGroupClient =
                new RestClient(_uriFormat, _service, operationGroup, _parameters);

            result = operationGroupClient;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            bool async = false;

            string operation = binder.Name;
            if (operation.EndsWith("Async", StringComparison.Ordinal)) {
                async = true;
                operation = operation.Substring(0, operation.Length - 5);
            }

            if (_operationGroup != null) {
                operation = _operationGroup + "." + operation;
            }

            if (async == false) {
                result = PerformOperation(operation, args);
            }
            else {
                result = PerformOperationAsync(operation, args);
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            if (_parameters == null) {
                _parameters = new Dictionary<string, object>();
            }
            _parameters[binder.Name] = value;
            return true;
        }
    }
}
