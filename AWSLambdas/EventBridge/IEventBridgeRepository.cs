using Amazon.EventBridge.Model;
using Amazon.SQS.Model;

namespace AWSLambdas.EventBridge
{
    public interface IEventBridgeRepository
    {
        Task<PutEventsResponse> PutEvent(string messageBody, string eventBusArn);
    }
}
