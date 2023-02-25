using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.IO;

namespace AWSLambdas.Services
{
    public class JsonConverter : IJsonConverter
    {
        private readonly JsonSerializer _jsonSerializer;
        public JsonConverter()
        {
            _jsonSerializer = new JsonSerializer();
        }

        public string SerializeObject(object obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                _jsonSerializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        public T DeserializeObject<T>(string json)
        {
            var stringReader = new StringReader(json);
            var jsonReader = new JsonTextReader(stringReader);
            return _jsonSerializer.Deserialize<T>(jsonReader);
        }
    }
}
