using Amazon.SimpleNotificationService.Model;

namespace AWSLambdas.SNS
{
    public class SnsRepository : ISnsRepository
    {
        private const string TopicArn = "arn:aws:sns:ap-northeast-1:178515926936:MessageFailed-FunctionExec";

        private readonly ISnsClient _snsClient;
        public SnsRepository(ISnsClient snsClient)
        {
            _snsClient = snsClient;

        }

        public async Task<PublishResponse> SendNotification(string emailSubject,string emailBody)
        {
            var publishRequest = new PublishRequest();
            publishRequest.TopicArn = TopicArn;
            publishRequest.Message = emailBody;
            publishRequest.Subject= emailSubject;
            return await _snsClient.SendNotification(publishRequest);
        }
    }
}
