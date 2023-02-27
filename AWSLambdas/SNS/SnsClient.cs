using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace AWSLambdas.SNS
{
    public class SnsClient : ISnsClient
    {
        private readonly AmazonSimpleNotificationServiceClient _snsClient;

        public SnsClient()
        {
            var snsConfig = new AmazonSimpleNotificationServiceConfig();
            snsConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _snsClient = new AmazonSimpleNotificationServiceClient(snsConfig);
        }

        public async Task<PublishResponse> SendNotification(PublishRequest publishRequest)
        {
            return await _snsClient.PublishAsync(publishRequest);
        }
    }
}
