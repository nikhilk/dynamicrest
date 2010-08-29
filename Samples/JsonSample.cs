// JsonSample.cs
//

using System;
using DynamicRest;

namespace Application {

    internal static class JsonSample {

        public static void Run() {
            string jsonText = "{ xyz: 123, items: [ 10, 100, 1000 ] }";

            JsonReader jsonReader = new JsonReader(jsonText);
            dynamic jsonObject = jsonReader.ReadValue();
            dynamic items = jsonObject.items;

            items[2] = 1001;

            dynamic bar = new JsonObject();
            bar.name = "c#";

            jsonObject.bar = bar;

            JsonWriter writer = new JsonWriter();
            writer.WriteValue((object)jsonObject);

            string newJsonText = writer.Json;

            Console.WriteLine(newJsonText);
        }
    }
}
