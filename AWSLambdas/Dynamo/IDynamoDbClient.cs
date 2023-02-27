using Amazon.DynamoDBv2.Model;

namespace AWSLambdas.Dynamo
{
    public interface IDynamoDbClient
    {
        Task PutItemAsync(PutItemRequest putItemRequest);

        Task<QueryResponse> QueryAsync(QueryRequest queryRequest);

        Task<UpdateItemResponse> UpdateItem(UpdateItemRequest updateItemRequest);
    }
}
