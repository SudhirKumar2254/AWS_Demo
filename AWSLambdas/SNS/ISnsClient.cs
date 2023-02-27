using Amazon.SimpleNotificationService.Model;

namespace AWSLambdas.SNS
{
    public interface ISnsClient
    {
        Task<PublishResponse> SendNotification(PublishRequest publishRequest);
    }
}
