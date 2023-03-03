using Amazon.EventBridge.Model;
using Amazon.SQS.Model;

namespace AWSLambdas.EventBridge
{
    public class EventBridgeRepository : IEventBridgeRepository
    {
        private readonly IEventBridgeClient _eventBridgeClient;
        public EventBridgeRepository(IEventBridgeClient eventBridgeClient)
        {
            _eventBridgeClient = eventBridgeClient;

        }

        public async Task<PutEventsResponse> PutEvent(string messageBody, string eventBusArn)
        {
            var putEventsRequestEntry = new PutEventsRequestEntry();
            putEventsRequestEntry.Detail = messageBody;
            putEventsRequestEntry.EventBusName = eventBusArn;
            putEventsRequestEntry.Source = "TriggeredFromDocGenClient";
            putEventsRequestEntry.DetailType = "lambda";

            List<PutEventsRequestEntry> putEventsRequestEntries = new List<PutEventsRequestEntry>();
            putEventsRequestEntries.Add(putEventsRequestEntry);

            var putEventsRequest = new PutEventsRequest();
            putEventsRequest.Entries = putEventsRequestEntries;
            return await _eventBridgeClient.PutEvent(putEventsRequest);
        }
    }
}
