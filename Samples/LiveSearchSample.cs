// LiveSearchSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class LiveSearchSample {

        public static void Run() {
            dynamic liveSearch = new RestClient(AppSettings.Default.LiveSearchUri, RestClientMode.Json);
            liveSearch.appID = AppSettings.Default.LiveSearchApiKey;

            Console.WriteLine("Searching Live for 'seattle'...");

            dynamic searchOptions = new JsonObject();
            searchOptions.Query = "seattle";
            dynamic response = liveSearch(searchOptions);

            dynamic results = response.SearchResponse.Web.Results;
            foreach (dynamic item in results) {
                Console.WriteLine(item.Title);
                Console.WriteLine(item.DisplayUrl);
                Console.WriteLine();
            }
        }
    }
}
