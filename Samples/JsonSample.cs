// JsonSample.cs
// DynamicRest provides REST service access using C# 4.0 dynamic programming.
// The latest information and code for the project can be found at
// https://github.com/NikhilK/dynamicrest
//
// This project is licensed under the BSD license. See the License.txt file for
// more information.
//

using System;
using DynamicRest;

namespace Application {

    internal static class JsonSample {

        public static void Run() {
            string jsonText = "{ xyz: 123, items: [ 10, 100, 1000 ], numbers: [ 0.123, .456 ], bools: [ true, false ], text: [ \"hello\", 'world\n!', \"\\\"s\\\"\", \"'s'\" ] }";

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
