using Amazon.StepFunctions.Model;

namespace AWSLambdas.StepFunctions
{
    public class StepFunctionsRepository : IStepFunctionsRepository
    {
        private const string StateMachineArn = "arn:aws:states:ap-northeast-1:178515926936:stateMachine:EmployeeDetailsPostingStateMachine";

        private readonly IStepFunctionsClient _stepFunctionsClient;
        public StepFunctionsRepository(IStepFunctionsClient stepFunctionsClient)
        {
            _stepFunctionsClient = stepFunctionsClient;

        }


        public async Task<StartExecutionResponse> StartExecution()

        {
            var startExecutionRequest= new StartExecutionRequest();
            startExecutionRequest.StateMachineArn = StateMachineArn;
            return await _stepFunctionsClient.StartExecution(startExecutionRequest);
        }
    }
}
