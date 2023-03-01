using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace AWSLambdas.SQS
{
    public class SqsClient : ISqsClient
    {
        private readonly AmazonSQSClient _sqsClient;

        public SqsClient()
        {
            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _sqsClient = new AmazonSQSClient(sqsConfig);
        }

        public async Task<SendMessageResponse> SendMessageToSQSQueue(SendMessageRequest sendMessageRequest)
        {
            return await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}
