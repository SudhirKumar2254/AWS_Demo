using Amazon.Lambda.Model;

namespace AWSLambdas.Lambda
{
    public interface ILambdaClient
    {
        Task<InvokeResponse> Invoke(InvokeRequest invokeAsyncRequest);
    }
}
