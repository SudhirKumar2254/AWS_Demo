using Amazon.DynamoDBv2.Model;
using AWSLambdas.Models;

namespace AWSLambdas.Dynamo
{
    public class EmployeeMessagesRepository : IEmployeeMessagesRepository
    {
        private const string TableName = "EmployeeMessages";

        private readonly IDynamoDbClient _databaseClient;
        public EmployeeMessagesRepository(IDynamoDbClient databaseClient)
        {
            _databaseClient = databaseClient;

        }
        public async Task SaveEmployeeMessagesAsync(EmployeeMessageModel employeeMessage)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
            {
                {"Message",new AttributeValue{S= employeeMessage.Message } }
            }
            };

            await _databaseClient.PutItemAsync(request);
        }
    }
}
