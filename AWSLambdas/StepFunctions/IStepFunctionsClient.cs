using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public interface IStepFunctionsClient
    {
        Task<StartExecutionResponse> StartExecution(StartExecutionRequest startExecutionRequest);
    }
}
