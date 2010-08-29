// GoogleSearchSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class GoogleSearchSample {

        public static void Run() {
            dynamic googleSearch = new RestClient(Services.GoogleSearchUri, RestClientMode.Json);

            Console.WriteLine("Searching Google for 'seattle'...");

            dynamic searchOptions = new JsonObject();
            searchOptions.q = "seattle";
            dynamic response = googleSearch(searchOptions);

            dynamic results = response.responseData.results;
            foreach (dynamic item in results) {
                Console.WriteLine(item.titleNoFormatting);
                Console.WriteLine(item.url);
                Console.WriteLine();
            }
        }
    }
}
