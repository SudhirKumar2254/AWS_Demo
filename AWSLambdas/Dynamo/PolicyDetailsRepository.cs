using Amazon.DynamoDBv2.Model;
using AWSLambdas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdas.Dynamo
{
    public class PolicyDetailsRepository : IPolicyDetailsRepository
    {
        private const string TableName = "EmployeeDetails";

        private readonly IDynamoDbClient _databaseClient;
        public PolicyDetailsRepository(IDynamoDbClient databaseClient)
        {
            _databaseClient = databaseClient;

        }
        public async Task SaveEmployeeDetailsAsync(EmployeeDetailsModel employeeDetails)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
            {
                {"Name",new AttributeValue{S= employeeDetails.Name } },
                {"Designation",new AttributeValue{S= employeeDetails.Designation } }
            }
            };

            await _databaseClient.PutItemAsync(request);
        }
    }
}
