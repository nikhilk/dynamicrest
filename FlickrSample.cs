// FlickrSample.cs
//

using System;
using DynamicRest;
using Services;

public static class FlickrSample {

    private static void WritePhoto(dynamic photo) {
        Console.WriteLine(photo.title);
        Console.WriteLine(String.Format("http://farm{0}.static.flickr.com/{1}/{2}_{3}.jpg",
                                        photo.farm, photo.server, photo.id, photo.secret));
        Console.WriteLine();
    }

    public static void Run() {
        dynamic flickr = new Flickr("be9b6f66bd7a1c0c0f1465a1b7e8a764");

        Console.WriteLine("Searching photos tagged with 'seattle'...");

        dynamic photosOptions = new JsonObject();
        photosOptions.tags = "seattle";
        dynamic searchResponse = flickr.Photos.Search(photosOptions);

        dynamic photos = searchResponse.photos.photo;
        foreach (dynamic photo in photos) {
            WritePhoto(photo);
        }


        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("Searching interesting photos...");

        dynamic interestingList = flickr.Interestingness.GetList();
        photos = interestingList.photos.photo;
        foreach (dynamic photo in photos) {
            WritePhoto(photo);
        }
    }
}
