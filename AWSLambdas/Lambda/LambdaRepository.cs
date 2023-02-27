using Amazon.Lambda.Model;

namespace AWSLambdas.Lambda
{
    public class LambdaRepository : ILambdaRepository
    {
        private const string StateMachineArn = "arn:aws:states:ap-northeast-1:178515926936:stateMachine:EmployeeDetailsPostingStateMachine";

        private readonly ILambdaClient _lambdaClient;
        public LambdaRepository(ILambdaClient lambdaClient)
        {
            _lambdaClient = lambdaClient;

        }


        public async Task<InvokeResponse> Invoke(string functionName, string payload)

        {
            var invokeRequest = new InvokeRequest();
            invokeRequest.FunctionName = functionName;
            invokeRequest.Payload = payload;
            return await _lambdaClient.Invoke(invokeRequest);
        }

    }
}
