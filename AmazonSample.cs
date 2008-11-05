// AmazonSample.cs
//

using System;
using DynamicRest;
using Services;

public static class AmazonSample {

    public static void Run() {
        dynamic amazon = new Amazon("1S494ZXP934WJ38NH5R2");
        dynamic bookOptions = new JsonObject();
        bookOptions.SearchIndex = "Books";
        bookOptions.Keywords = "Dynamic Programming";
        dynamic bookList = amazon.ItemSearch(bookOptions);

        foreach (dynamic book in bookList.SelectAll("Item")) {
            Console.WriteLine(book.ASIN + " : " + book.ItemAttributes.Title);
        }
    }
}
