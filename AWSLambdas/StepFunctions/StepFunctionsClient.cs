using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public class StepFunctionsClient : IStepFunctionsClient
    {
        private readonly IAmazonStepFunctions _stepFunctionsClient;

        public StepFunctionsClient()
        {
            var stepFuncConfig = new AmazonStepFunctionsConfig();
            stepFuncConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _stepFunctionsClient = new AmazonStepFunctionsClient(stepFuncConfig);
        }


        public async Task<StartExecutionResponse> StartExecution(StartExecutionRequest startExecutionRequest)
        {
            return await _stepFunctionsClient.StartExecutionAsync(startExecutionRequest);
        }

        public async Task<DescribeExecutionResponse> DescribeExecution(DescribeExecutionRequest describeExecutionRequest)
        {
            return await _stepFunctionsClient.DescribeExecutionAsync(describeExecutionRequest);
        }
    }
}
