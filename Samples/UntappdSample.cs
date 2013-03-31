// UntappdSample.cs
// DynamicRest provides REST service access using C# 4.0 dynamic programming.
// The latest information and code for the project can be found at
// https://github.com/NikhilK/dynamicrest
//
// This project is licensed under the BSD license. See the License.txt file for
// more information.
//

using System;
using DynamicRest;
using System.Collections.Generic;
using System.Text;

namespace Application {

    internal static class UntappdSample {

        public static void Run() {
            List<string> PostOps = new List<string>()
            {
                "checkin_test",
            };
            dynamic untappd = new RestClient(Services.UntappdUri, RestService.Json, PostOps);
            untappd.apiKey = Services.UntappdApiKey;
			untappd = untappd.WithBasicAuth(Services.UntappdCheckinAs, MD5(Services.UntappdCheckinAsPassword));
			
            Console.WriteLine("Checking in");

            dynamic checkinOptions = new JsonObject();
            checkinOptions.gmt_offset = -8;
            checkinOptions.bid = 1;
			
			dynamic checkin = untappd.checkin_test(checkinOptions);
			Console.WriteLine(string.Format("That was checkin ID #{0}",checkin.Result.checkin_details.checkin_id));
        }

		private static string MD5(string Input)
        {
            // from http://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx
            System.Security.Cryptography.MD5 Md5Hasher = System.Security.Cryptography.MD5.Create();
            byte[] data = Md5Hasher.ComputeHash(Encoding.Default.GetBytes(Input));
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
