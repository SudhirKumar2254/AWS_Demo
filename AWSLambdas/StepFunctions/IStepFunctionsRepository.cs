using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public interface IStepFunctionsRepository
    {
        Task<StartExecutionResponse> StartExecution();

        Task<DescribeExecutionResponse> DescribeExecution(string executionArn);
    }
}
