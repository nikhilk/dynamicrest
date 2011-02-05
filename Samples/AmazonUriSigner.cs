// AmazonUriSigner.cs
// DynamicRest provides REST service access using C# 4.0 dynamic programming.
// The latest information and code for the project can be found at
// https://github.com/NikhilK/dynamicrest
//
// This project is licensed under the BSD license. See the License.txt file for
// more information.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DynamicRest;

namespace Application {

    internal sealed class AmazonUriSigner : IRestUriTransformer {

        private string _appKey;
        private byte[] _secret;
        private HMAC _hmac;

        public AmazonUriSigner(string appKey, string secretKey) {
            _appKey = appKey;
            _secret = Encoding.UTF8.GetBytes(secretKey);
            _hmac = new HMACSHA256(_secret);
        }

        private string ConstructQueryString(SortedDictionary<string, string> parameters) {
            if (parameters.Count == 0) {
                return String.Empty;
            }

            StringBuilder queryStringBuilder = new StringBuilder();
            int parameter = 0;
            foreach (KeyValuePair<string, string> kvp in parameters) {
                if (parameter != 0) {
                    queryStringBuilder.Append("&");
                }

                queryStringBuilder.Append(kvp.Key);
                queryStringBuilder.Append("=");
                queryStringBuilder.Append(PercentEncodeRfc3986(kvp.Value));

                parameter++;
            }

            return queryStringBuilder.ToString();
        }

        private SortedDictionary<string, string> ParseQueryString(string queryString) {
            // Use a SortedDictionary to get the parameters in natural byte order
            SortedDictionary<string, string> parameters =
                new SortedDictionary<string, string>(new ParameterComparer());

            char[] queryStringSeparator = new char[] { '&' };
            char[] parameterSeparator = new char[] { '=' };

            string[] queryStringParts = queryString.Split(queryStringSeparator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < queryStringParts.Length; i++) {
                string[] nameValueParts = queryStringParts[i].Split(parameterSeparator);
                parameters[nameValueParts[0]] = HttpUtility.UrlDecode(nameValueParts[1]);
            }

            return parameters;
        }

        private string PercentEncodeRfc3986(string s) {
            // Percent-encode (URL Encode) according to RFC 3986.
            // This is necessary because .NET's HttpUtility.UrlEncode does not encode
            // according to the above standard.

            s = HttpUtility.UrlEncode(s).
                            Replace("'", "%27").
                            Replace("(", "%28").
                            Replace(")", "%29").
                            Replace("*", "%2A").
                            Replace("!", "%21").
                            Replace("%7e", "~").
                            Replace("+", "%20");

            StringBuilder sb = new StringBuilder(s);
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] == '%') {
                    if (Char.IsDigit(sb[i + 1]) && Char.IsLetter(sb[i + 2])) {
                        sb[i + 2] = Char.ToUpper(sb[i + 2], CultureInfo.InvariantCulture);
                    }
                }
            }

            return sb.ToString();
        }

        private string SignCore(string endPoint, string requestPath, SortedDictionary<string, string> parameters) {
            // Add the AWSAccessKeyId and Timestamp (in IS0 8601 format) to the requests.
            parameters["AWSAccessKeyId"] = _appKey;
            parameters["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string queryString = ConstructQueryString(parameters);

            // Derive the bytes that need to be signed
            // (request method, domain, path and the query string)
            StringBuilder dataBuilder = new StringBuilder();
            dataBuilder.Append("GET").
                Append("\n").
                Append(endPoint).
                Append("\n").
                Append(requestPath).
                Append("\n").
                Append(queryString);

            byte[] requestBytes = Encoding.UTF8.GetBytes(dataBuilder.ToString());

            // Compute the signature and convert to Base64.
            byte[] signedRequestBytes = _hmac.ComputeHash(requestBytes);
            string signature = Convert.ToBase64String(signedRequestBytes);

            // Construct the complete URL
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append("http://").
                Append(endPoint).
                Append(requestPath).
                Append("?").
                Append(queryString).
                Append("&Signature=").
                Append(PercentEncodeRfc3986(signature));

            return urlBuilder.ToString();
        }

        #region Implementation of IRestUriTransformer
        Uri IRestUriTransformer.TransformUri(Uri uri) {
            string endPoint = uri.Host.ToLowerInvariant();
            string requestPath = uri.AbsolutePath;
            string queryString = uri.Query;
            if (queryString.Length > 1) {
                Debug.Assert(queryString.StartsWith("?"));
                queryString = queryString.Substring(1);
            }

            SortedDictionary<string, string> query = ParseQueryString(queryString);

            string signedUrl = SignCore(endPoint, requestPath, query);
            return new Uri(signedUrl, UriKind.Absolute);
        }
        #endregion


        private sealed class ParameterComparer : IComparer<string> {

            public int Compare(string p1, string p2) {
                return String.CompareOrdinal(p1, p2);
            }
        }
    }
}
