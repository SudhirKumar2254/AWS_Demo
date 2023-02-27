using Amazon.SimpleNotificationService.Model;

namespace AWSLambdas.SNS
{
    public interface ISnsRepository
    {
        Task<PublishResponse> SendNotification(string emailSubject, string emailBody);
    }
}
