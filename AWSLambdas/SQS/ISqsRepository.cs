using Amazon.SQS.Model;

namespace AWSLambdas.SQS
{
    public interface ISqsRepository
    {
        Task<SendMessageResponse> SendMessageToSQSQueue(string messageBody, string queueUrl);
    }
}
