// AmazonSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class AmazonSample {

        public static void Run() {
            dynamic amazon = new RestClient(AppSettings.Default.AmazonUri, RestClientMode.Xml);
            amazon.subscriptionID = AppSettings.Default.AmazonSubscriptionID;

            dynamic searchOptions = new JsonObject();
            searchOptions.SearchIndex = "Books";
            searchOptions.Keywords = "Dynamic Programming";

            dynamic bookList = amazon.ItemSearch(searchOptions);

            foreach (dynamic book in bookList.SelectAll("Item")) {
                Console.WriteLine(book.ASIN + " : " + book.ItemAttributes.Title);
            }
        }
    }
}
