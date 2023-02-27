using Amazon.Lambda.Model;

namespace AWSLambdas.Lambda
{
    public interface ILambdaRepository
    {
        Task<InvokeResponse> Invoke(string functionName, string payload);
    }
}
