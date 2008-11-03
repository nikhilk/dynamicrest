// Program.cs
//

using System;
using DynamicRest;

class Program {

    public static void Main(string[] args) {
        string jsonText = "{ xyz: 123, items: [ 10, 100, 1000 ] }";

        JsonReader jsonReader = new JsonReader(jsonText);
        dynamic jsonObject = jsonReader.ReadValue();
        dynamic items = jsonObject.items;

        items.Item(2, 1001);

        dynamic bar = new JsonObject();
        bar.name = "c#";

        jsonObject.bar = bar;

        JsonWriter writer = new JsonWriter();
        writer.WriteValue((object)jsonObject);

        string newJsonText = writer.Json;

        Console.WriteLine(newJsonText);
    }
}
