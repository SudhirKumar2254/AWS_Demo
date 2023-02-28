using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public interface IStepFunctionsRepository
    {
        Task<StartExecutionResponse> StartExecution(string input);

        Task<DescribeExecutionResponse> DescribeExecution(string executionArn);
    }
}
