using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public class StepFunctionsRepository : IStepFunctionsRepository
    {
        private const string StateMachineArn = "arn:aws:states:ap-northeast-1:178515926936:stateMachine:WorkflowAfterPostBindClient";

        private readonly IStepFunctionsClient _stepFunctionsClient;
        public StepFunctionsRepository(IStepFunctionsClient stepFunctionsClient)
        {
            _stepFunctionsClient = stepFunctionsClient;

        }


        public async Task<StartExecutionResponse> StartExecution(string input)

        {
            var startExecutionRequest = new StartExecutionRequest();
            startExecutionRequest.StateMachineArn = StateMachineArn;
            startExecutionRequest.Input = input;
            return await _stepFunctionsClient.StartExecution(startExecutionRequest);
        }

        public async Task<DescribeExecutionResponse> DescribeExecution(string executionArn)

        {
            var describeExecutionRequest = new DescribeExecutionRequest();
            describeExecutionRequest.ExecutionArn = executionArn;
            return await _stepFunctionsClient.DescribeExecution(describeExecutionRequest);
        }
    }
}
