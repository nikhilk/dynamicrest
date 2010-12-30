// BingSearchSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class BingSearchSample {

        public static void Run() {
            dynamic bingSearch = new RestClient(Services.BingSearchUri, RestService.Json);
            bingSearch.appID = Services.BingApiKey;

            Console.WriteLine("Searching Bing for 'seattle'...");

            dynamic searchOptions = new JsonObject();
            searchOptions.Query = "seattle";
            searchOptions.Sources = new string[] { "Web", "Image" };
            searchOptions.Web = new JsonObject("Count", 4);
            searchOptions.Image = new JsonObject("Count", 2);

            dynamic search = bingSearch.invoke(searchOptions);
            dynamic searchResponse = search.Result.SearchResponse;

            foreach (dynamic item in searchResponse.Web.Results) {
                Console.WriteLine(item.Title);
                Console.WriteLine(item.DisplayUrl);
                Console.WriteLine();
            }
            foreach (dynamic item in searchResponse.Image.Results) {
                Console.WriteLine(item.Title);
                Console.WriteLine(item.MediaUrl);
                Console.WriteLine();
            }
        }
    }
}
