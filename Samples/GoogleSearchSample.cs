// GoogleSearchSample.cs
//

using System;
using DynamicRest;
using System.Threading;

namespace Application {

    internal static class GoogleSearchSample {

        public static void Run() {
            dynamic googleSearch = new RestClient(Services.GoogleSearchUri, RestService.Json);

            Console.WriteLine("Searching Google for 'seattle'...");

            dynamic searchOptions = new JsonObject();
            searchOptions.q = "seattle";

            dynamic search = googleSearch.invokeAsync(searchOptions);
            search.Callback((RestCallback)delegate() {
                dynamic results = search.Result.responseData.results;
                foreach (dynamic item in results) {
                    Console.WriteLine(item.titleNoFormatting);
                    Console.WriteLine(item.url);
                    Console.WriteLine();
                }
            });


            while (search.IsCompleted == false) {
                Console.WriteLine(".");
                Thread.Sleep(100);
            }
        }
    }
}
