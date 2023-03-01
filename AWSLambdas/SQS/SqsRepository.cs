using Amazon.SQS.Model;
using AWSLambdas.SQS;

namespace AWSLambdas.SQS
{
    public class SqsRepository : ISqsRepository
    {
        private readonly ISqsClient _sqsClient;
        public SqsRepository(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;

        }

        public async Task<SendMessageResponse> SendMessageToSQSQueue(string messageBody, string queueUrl)
        {
            var messageRequest = new SendMessageRequest();
            messageRequest.MessageBody = messageBody;
            messageRequest.QueueUrl = queueUrl;
            return await _sqsClient.SendMessageToSQSQueue(messageRequest);
        }
    }
}
