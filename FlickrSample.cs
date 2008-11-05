// FlickrSample.cs
//

using System;
using DynamicRest;
using Services;

public static class FlickrSample {

    public static void Run() {
        dynamic flickr = new Flickr("be9b6f66bd7a1c0c0f1465a1b7e8a764");

        Console.WriteLine("Searching photos tagged with 'seattle'...");

        dynamic photosOptions = new JsonObject();
        photosOptions.tags = "seattle";
        dynamic searchResponse = flickr.Photos.Search(photosOptions);

        dynamic photos = searchResponse.photos.photo;
        foreach (dynamic photo in photos) {
            Console.WriteLine(photo.title);
        }


        Console.WriteLine("Searching interesting photos...");

        dynamic interestingList = flickr.Interestingness.GetList();
        photos = interestingList.photos.photo;
        foreach (dynamic photo in photos) {
            Console.WriteLine(photo.title);
        }
    }
}
