using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace NDomain.Azure.Bus.Transport
{
    internal class Serializer
    {
        public static Stream Serialize(JObject obj)
        {
            var stream = new MemoryStream();
            using (var jsonWriter = new BsonWriter(stream))
            {
                jsonWriter.CloseOutput = false;
                obj.WriteTo(jsonWriter);
            }

            stream.Position = 0;

            return stream;
        }

        public static JObject Deserialize(Stream stream)
        {
            JObject obj;
            using (var jsonReader = new BsonReader(stream))
            {
                jsonReader.CloseInput = true;
                obj = JObject.Load(jsonReader);
            }

            return obj;
        }
    }
}
