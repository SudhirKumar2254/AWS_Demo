using Amazon.EventBridge.Model;

namespace AWSLambdas.EventBridge
{
    public interface IEventBridgeClient
    {
        Task<PutEventsResponse> PutEvent(PutEventsRequest putEventsRequest);
    }
}
