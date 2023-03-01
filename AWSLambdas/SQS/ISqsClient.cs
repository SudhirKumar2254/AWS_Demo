using Amazon.SQS.Model;

namespace AWSLambdas.SQS
{
    public interface ISqsClient
    {
        Task<SendMessageResponse> SendMessageToSQSQueue(SendMessageRequest sendMessageRequest);
    }
}
