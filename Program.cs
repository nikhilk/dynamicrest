// Program.cs
//

using System;

namespace Application {

    public static class Program {

        private static readonly string HeaderBar = new String('=', 79);
        private static readonly string Separator = new String('-', 79);

        public static void Main(string[] args) {
            // NOTE: To run the samples, you'll need a few API keys
            //       that you need to specify in Services.cs.

            // JSON Sample
            Console.WriteLine("JSON Sample");
            Console.WriteLine(HeaderBar);
            JsonSample.Run();
            Console.WriteLine(Separator);
            Console.WriteLine(Environment.NewLine);

            // Flickr Sample
            Console.WriteLine("Flickr Sample");
            Console.WriteLine(HeaderBar);
            FlickrSample.Run();
            Console.WriteLine(Separator);
            Console.WriteLine(Environment.NewLine);

            // Amazon Sample
            Console.WriteLine("Amazon Sample");
            Console.WriteLine(HeaderBar);
            AmazonSample.Run();
            Console.WriteLine(Separator);
            Console.WriteLine(Environment.NewLine);

            // Bing Search
            Console.WriteLine("Bing Sample");
            Console.WriteLine(HeaderBar);
            BingSearchSample.Run();
            Console.WriteLine(Separator);
            Console.WriteLine(Environment.NewLine);

            // Google Search
            Console.WriteLine("Google Search Sample");
            Console.WriteLine(HeaderBar);
            GoogleSearchSample.Run();
            Console.WriteLine(Separator);

            Console.ReadLine();
        }
    }
}
