namespace AWSLambdas.Services
{
    public interface IJsonConverter
    {
        string SerializeObject (object obj);
        T DeserializeObject<T>(string  json);
    }
}
