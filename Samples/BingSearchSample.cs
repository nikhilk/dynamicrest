// BingSearchSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class BingSearchSample {

        public static void Run() {
            dynamic bingSearch = new RestClient(Services.BingSearchUri, RestClientMode.Json);
            bingSearch.appID = Services.BingApiKey;

            Console.WriteLine("Searching Live for 'seattle'...");

            dynamic searchOptions = new JsonObject();
            searchOptions.Query = "seattle";

            dynamic response = bingSearch(searchOptions);

            dynamic results = response.SearchResponse.Web.Results;
            foreach (dynamic item in results) {
                Console.WriteLine(item.Title);
                Console.WriteLine(item.DisplayUrl);
                Console.WriteLine();
            }
        }
    }
}
