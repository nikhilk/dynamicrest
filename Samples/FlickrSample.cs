// FlickrSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class FlickrSample {

        private static void WritePhotos(dynamic list) {
            foreach (dynamic photo in list.photos.photo) {
                Console.WriteLine(photo.title);
                Console.WriteLine(String.Format("http://farm{0}.static.flickr.com/{1}/{2}_{3}.jpg",
                                                photo.farm, photo.server, photo.id, photo.secret));
                Console.WriteLine();
            }
        }

        public static void Run() {
            dynamic flickr = new RestClient(Services.FlickrUri, RestClientMode.Json);
            flickr.apiKey = Services.FlickrApiKey;

            Console.WriteLine("Searching photos tagged with 'seattle'...");

            dynamic photosOptions = new JsonObject();
            photosOptions.tags = "seattle";
            photosOptions.per_page = 4;
            dynamic searchResponse = flickr.Photos.Search(photosOptions);
            WritePhotos(searchResponse);

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Searching interesting photos...");

            dynamic interestingnessOptions = new JsonObject();
            interestingnessOptions.per_page = 4;
            dynamic interestingList = flickr.Interestingness.GetList(interestingnessOptions);
            WritePhotos(interestingList);
        }
    }
}
