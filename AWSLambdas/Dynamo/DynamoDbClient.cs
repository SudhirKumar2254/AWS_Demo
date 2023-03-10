using Amazon;
using Amazon.Auth.AccessControlPolicy;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AWSLambdas.Dynamo
{
    public class DynamoDbClient : IDynamoDbClient
    {
        private readonly IAmazonDynamoDB _amazonDynamoDBClient;

        public DynamoDbClient()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.APNortheast1;
            _amazonDynamoDBClient = new AmazonDynamoDBClient(dynamoDbConfig);
        }


        public async Task PutItemAsync(PutItemRequest putItemRequest)
        {
            await _amazonDynamoDBClient.PutItemAsync(putItemRequest);
        }

        public async Task<QueryResponse> QueryAsync(QueryRequest queryRequest)
        {
            return await _amazonDynamoDBClient.QueryAsync(queryRequest);
        }

        public async Task<UpdateItemResponse> UpdateItem(UpdateItemRequest updateItemRequest)
        {
            return await _amazonDynamoDBClient.UpdateItemAsync(updateItemRequest);
        }

        public async Task<ScanResponse> GetAllItems(ScanRequest scanRequest)
        {
            return await _amazonDynamoDBClient.ScanAsync(scanRequest);
        }
        public async Task DeleteItemAsync(DeleteItemRequest deleteItemRequest)
        {
            await _amazonDynamoDBClient.DeleteItemAsync(deleteItemRequest);
        }
    }
}
