using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

namespace AWSLambdas.Lambda
{
    public class LambdaClient : ILambdaClient
    {
        private readonly IAmazonLambda _lambdaClient;

        public LambdaClient()
        {
            var lamdbdaConfig = new AmazonLambdaConfig();
            lamdbdaConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _lambdaClient = new AmazonLambdaClient(lamdbdaConfig);
        }


        public async Task<InvokeResponse> Invoke(InvokeRequest invokeAsyncRequest)
        {
            return await _lambdaClient.InvokeAsync(invokeAsyncRequest);
        }

    }
}
