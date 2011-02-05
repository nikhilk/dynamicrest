// AmazonSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class AmazonSample {

        public static void Run() {
            AmazonUriSigner signer = new AmazonUriSigner(Services.AmazonAccessKey, Services.AmazonSecretKey);
            dynamic amazon = new RestClient(Services.AmazonUri, RestService.Xml).
                                 WithUriTransformer(signer);

            dynamic searchOptions = new JsonObject();
            searchOptions.SearchIndex = "Books";
            searchOptions.Keywords = "Dynamic Programming";

            dynamic search = amazon.ItemSearch(searchOptions);
            dynamic bookList = search.Result;

            foreach (dynamic book in bookList.SelectAll("Item")) {
                Console.WriteLine(book.ASIN + " : " + book.ItemAttributes.Title);
            }
        }
    }
}
