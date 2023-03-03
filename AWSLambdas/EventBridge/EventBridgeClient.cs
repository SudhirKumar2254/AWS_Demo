using Amazon;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace AWSLambdas.EventBridge
{
    public class EventBridgeClient : IEventBridgeClient
    {
        private readonly AmazonEventBridgeClient _eventBridgeClient;

        public EventBridgeClient()
        {
            var eventBridgeConfig = new AmazonEventBridgeConfig();
            eventBridgeConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _eventBridgeClient = new AmazonEventBridgeClient(eventBridgeConfig);
        }

        public async Task<PutEventsResponse> PutEvent(PutEventsRequest putEventsRequest)
        {
            return await _eventBridgeClient.PutEventsAsync(putEventsRequest);
        }
    }
}
