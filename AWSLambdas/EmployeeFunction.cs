using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime.EventStreams;
using Amazon.Runtime.Internal;
using Amazon.SQS;
using AWSLambdas.Dynamo;
using AWSLambdas.Lambda;
using AWSLambdas.Models;
using AWSLambdas.Services;
using AWSLambdas.SNS;
using AWSLambdas.SQS;
using AWSLambdas.StepFunctions;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.ExceptionServices;
using Amazon.EventBridge;
using AWSLambdas.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.Lambda.CloudWatchEvents;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdas;

public class EmployeeFunction
{

    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IJsonConverter _jsonConverter;
    private readonly IEmployeeMessagesRepository _employeeMessagesRepository;
    private readonly IStepFunctionsClient _stepFunctionsClient;
    private readonly IStepFunctionsRepository _stepFunctionsRepository;
    private readonly ISnsClient _snsClient;
    private readonly ISnsRepository _snsRepository;
    private readonly ILambdaClient _lambdaClient;
    private readonly ILambdaRepository _lambdaRepository;
    private readonly ISqsClient _sqsClient;
    private readonly ISqsRepository _sqsRepository;
    private readonly IEventBridgeClient _eventBridgeClient;
    private readonly IEventBridgeRepository _eventBridgeRepository;
    public EmployeeFunction() : this(null, null, null, null, null, null, null, null, null, null, null, null, null) { }

    public EmployeeFunction(IJsonConverter jsonConverter, IDynamoDbClient dynamoDbClient, IEmployeeMessagesRepository employeeMessagesRepository, IStepFunctionsClient stepFunctionsClient, IStepFunctionsRepository stepFunctionsRepository, ISnsClient snsClient, ISnsRepository snsRepository, ILambdaClient lambdaClient, ILambdaRepository lambdaRepository, ISqsClient sqsClient, ISqsRepository sqsRepository, IEventBridgeClient eventBridgeClient, IEventBridgeRepository eventBridgeRepository)
    {
        _jsonConverter = jsonConverter ?? new JsonConverter();
        _dynamoDbClient = dynamoDbClient ?? new DynamoDbClient();
        _employeeMessagesRepository = employeeMessagesRepository ?? new EmployeeMessagesRepository(_dynamoDbClient);
        _stepFunctionsClient = stepFunctionsClient ?? new StepFunctionsClient();
        _stepFunctionsRepository = stepFunctionsRepository ?? new StepFunctionsRepository(_stepFunctionsClient);
        _snsClient = snsClient ?? new SnsClient();
        _snsRepository = snsRepository ?? new SnsRepository(_snsClient);
        _lambdaClient = lambdaClient ?? new LambdaClient();
        _lambdaRepository = lambdaRepository ?? new LambdaRepository(_lambdaClient);
        _sqsClient = sqsClient ?? new SqsClient();
        _sqsRepository = sqsRepository ?? new SqsRepository(_sqsClient);
        _eventBridgeClient = eventBridgeClient ?? new EventBridgeClient();
        _eventBridgeRepository = eventBridgeRepository ?? new EventBridgeRepository(_eventBridgeClient);
    }

    public async Task<APIGatewayProxyResponse> ValidateEmployeeDataHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.Log("Inside the ValidateEmployeeDataHanlder");
        var empDetails = _jsonConverter.DeserializeObject<EmployeeDetailsModel>(request.Body);

        //Validation of the input data
        if (string.IsNullOrEmpty(empDetails.Name))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Name is mandatory"
            };
        }

        if (string.IsNullOrEmpty(empDetails.Designation))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Designation is mandatory"
            };
        }


        //Check the ciruit breaker
        var queryRequest = new QueryRequest("EmployeeConfiguration")
        {
            KeyConditionExpression = "CircuitClosed = :CircuitClosed"
        };
        queryRequest.ExpressionAttributeValues.Add(":CircuitClosed", new AttributeValue { S = "Y" });

        var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

        //If circuit is closed do work else put the message in DB
        if (queryResponse.Count > 0)
        {
            var response = await PostData(request, context);
            SendSNSNotification(context, "Request processed successfully", "Your request is successfully processed... and the response is " + response.Body);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = response.Body
            };
        }
        else
        {

            PutMessageInDb(request, context);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Your request is under processing and you will get an email once processed. "
            };
        }
    }

    public void PutMessageInDb(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Inside the PutMessageInDb");

        EmployeeMessageModel employeeMessage = new EmployeeMessageModel();
        employeeMessage.Message = request.Body;
        _employeeMessagesRepository.SaveEmployeeMessagesAsync(employeeMessage);

    }

    public void SendSNSNotification(ILambdaContext context, string emailSubject, string emailBody)
    {
        context.Logger.LogLine("Inside SendSNSNotification");
        _snsRepository.SendNotification(emailSubject, emailBody);

    }

    public async Task<APIGatewayProxyResponse> PostData(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Inside the PostData");
        var empDetails = _jsonConverter.DeserializeObject<EmployeeDetailsModel>(request.Body);

        var response = await _stepFunctionsRepository.StartExecution("");
        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            Thread.Sleep(5000);
            var describeExecResponse = await _stepFunctionsRepository.DescribeExecution(response.ExecutionArn);
            if (describeExecResponse.Status == "SUCCEEDED")
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = describeExecResponse.Output
                };
            }
            else
            {
                PutMessageInDb(request, context);
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Your request failed..."
                };
            }
        }
        else
        {
            PutMessageInDb(request, context);
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "Your request failed"
        };
    }


    #region TMHCC-POC
    public async Task<APIGatewayProxyResponse> ValidateRequestAndCheckCircuitStatusHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Inside the ValidateRequestAndCheckCircuitStatusHandlder");
        var policyDetails = _jsonConverter.DeserializeObject<PolicyDetailsModel>(request.Body);

        //Validation of the input data
        if (string.IsNullOrEmpty(policyDetails.PolicyHolderName))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "PolicyHolderName is mandatory"
            };
        }

        if (policyDetails?.PolicyStartDate == null || policyDetails?.PolicyStartDate == DateTime.MinValue)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "PolicyStartDate is mandatory"
            };
        }

        if (policyDetails?.PolicyEndDate == null || policyDetails?.PolicyEndDate == DateTime.MinValue)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "PolicyEndDate is mandatory"
            };
        }

        //Check the ciruit breaker
        var queryRequest = new QueryRequest("CircuitBreakerDB")
        {
            KeyConditionExpression = "SettingName = :SettingName"
        };
        queryRequest.ExpressionAttributeValues.Add(":SettingName", new AttributeValue { S = "CircuitStatus" });

        queryRequest.FilterExpression = "CurrentStatus = :CurrentStatus";
        queryRequest.ExpressionAttributeValues.Add(":CurrentStatus", new AttributeValue { S = "Closed" });

        var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

        //If circuit is closed call post bind client else put the message in DB
        if (queryResponse.Count > 0)
        {
            //Call lambda function Post Bind Client
            var postBindClientResponse = await _lambdaRepository.Invoke("PostBindClient", request.Body);
            StreamReader reader = new StreamReader(postBindClientResponse.Payload);
            string postBindClientResponseText = reader.ReadToEnd();


            if (postBindClientResponseText.Contains("Post Bind Service failed"))
            {
                // Put this request in post bind queue
                var sqsResponse = await _sqsRepository.SendMessageToSQSQueue(request.Body, "https://sqs.ap-northeast-1.amazonaws.com/178515926936/PostBindQueue");

                //Send failure notification
                await _snsRepository.SendNotification("Post Bind Service failure", "Post Bind service caused issue. Please check and fix it");
                //Return generic message
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Your request is under processing and you will get an email once processed. PostBindQueue "
                };

            }
            // if post bind client is success invoke a step function async

            var response = await _stepFunctionsRepository.StartExecution(request.Body);


            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Your temporary policy id - " + postBindClientResponseText
            };
        }
        else
        {
            // Put the request in MessageDb
            await InsertRequestInMessagesDb(request.Body, request.RequestContext.RequestId);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Your request is under processing and you will get an email once processed. "
            };
        }
    }

    public async Task<string> PostBindClientHandler(AmazonLambdaRequest request, ILambdaContext context)
    {
        try
        {
            //throw new InvalidDataException();
            context.Logger.LogLine("Inside the PostBindClientHandler");
            //var policyDetails = _jsonConverter.DeserializeObject<PolicyDetailsModel>(request);

            HttpResponseMessage response = PostBindServiceCall();
            var policyDetails = _jsonConverter.DeserializeObject<PolicyDetailsModel>(response.Content.ReadAsStringAsync().Result);
            return policyDetails.Id.ToString();
        }
        catch (Exception ex)
        {
            context.Logger.LogLine("Error Message - " + ex.Message + "::::: Stack Trace - " + ex.StackTrace);

            //Update the ciruit breaker to Open
            await UpdateCircuitBreaker("Open");


            return "Post Bind Service failed";
        }
    }
    public async Task PostBindClientFromSQSHandler(SQSEvent evnt, ILambdaContext context)
    {
        context.Logger.LogLine($"Inside the PostBindClientFromSQSHandler");
        foreach (var message in evnt.Records)
        {

            //throw new ApplicationException("Intentionally failed");
            var policyDetails = _jsonConverter.DeserializeObject<PolicyDetailsModel>(message.Body);

            HttpResponseMessage response = PostBindServiceCall();
            // response.Content.ReadAsStringAsync().Result;    


            // if post bind client is success invoke a step function async
            var executionResponse = await _stepFunctionsRepository.StartExecution(message.Body);

            //Check the ciruit breaker
            var queryRequest = new QueryRequest("CircuitBreakerDB")
            {
                KeyConditionExpression = "SettingName = :SettingName"
            };
            queryRequest.ExpressionAttributeValues.Add(":SettingName", new AttributeValue { S = "CircuitStatus" });

            queryRequest.FilterExpression = "CurrentStatus = :CurrentStatus";
            queryRequest.ExpressionAttributeValues.Add(":CurrentStatus", new AttributeValue { S = "Closed" });

            var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);

            //If circuit is closed call post bind client else put the message in DB
            if (queryResponse.Count == 0)
            {
                //Update the ciruit breaker to Closed
                await UpdateCircuitBreaker("Closed");
            }

        }
    }
    private static HttpResponseMessage PostBindServiceCall()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://lvafns7zf8.execute-api.ap-northeast-1.amazonaws.com/Development/pets/1");
        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
        var response = client.Send(httpRequest);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("Post Bind Service not available");
        }
        return response;
    }

    private async Task UpdateCircuitBreaker(string value)
    {
        //Update the ciruit breaker to Open
        var updateItemRequest = new UpdateItemRequest();
        updateItemRequest.TableName = "CircuitBreakerDB";
        Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue> {
                {"SettingName",new AttributeValue{S = "CircuitStatus" } }
            };

        Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();

        updates["CurrentStatus"] = new AttributeValueUpdate() { Action = AttributeAction.PUT, Value = new AttributeValue { S = value } };

        updateItemRequest.Key = key;
        updateItemRequest.AttributeUpdates = updates;
        var updateResponse = await _dynamoDbClient.UpdateItem(updateItemRequest);
    }

    private async Task InsertRequestInMessagesDb(string message, string messageId)
    {
        //Update the ciruit breaker to Open
        var putItemRequest = new PutItemRequest();
        putItemRequest.TableName = "MessagesDb";
        Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue> {
                {"MessageId",new AttributeValue{S = messageId }},
                {"MessageDetails",new AttributeValue{S = message }}
            };
        putItemRequest.Item = item;
        await _dynamoDbClient.PutItemAsync(putItemRequest);
    }

    public async Task BackupMessagesProcessorHandler(GetRecordsResponse recordsList, ILambdaContext context)
    {
        context.Logger.LogLine($"Inside the BackupMessagesProcessorHandler");

        //throw new ApplicationException("Intentionally failed");
        //var policyDetails = _jsonConverter.DeserializeObject<PolicyDetailsModel>(message.Body);
        foreach (var item in recordsList.Records)
        {
            context.Logger.LogLine("Event:" + item.EventName.Value + "... Circuit Current Status:  " + item.Dynamodb.NewImage["CurrentStatus"].S);
            if (item.EventName.Value == "MODIFY" && item.Dynamodb.NewImage["CurrentStatus"].S == "Closed")
            {
                //Read all records from MessagesDb and process
                ScanRequest scanRequest = new ScanRequest();
                scanRequest.TableName = "MessagesDb";
                ScanResponse scanResponse = await _dynamoDbClient.GetAllItems(scanRequest);

                foreach (var row in scanResponse.Items)
                {
                    context.Logger.LogLine("Processing record messageId-" + row["MessageId"].S + "::: Message Details -" + row["MessageDetails"].S);
                    // Put this request in post bind queue
                    var sqsResponse = await _sqsRepository.SendMessageToSQSQueue(row["MessageDetails"].S, "https://sqs.ap-northeast-1.amazonaws.com/178515926936/PostBindQueue");
                    if (sqsResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        //Delete from Table once pushed to queue
                        var deleteItemRequest = new DeleteItemRequest();
                        deleteItemRequest.TableName = "MessagesDb";
                        Dictionary<string, AttributeValue> keys = new Dictionary<string, AttributeValue> {
                         {"MessageId",new AttributeValue{S =  row["MessageId"].S } }
                        };
                        deleteItemRequest.Key = keys;
                        await _dynamoDbClient.DeleteItemAsync(deleteItemRequest);
                    }
                }
            }

        }
    }

    public async Task DocumentGenClientFromSQSHandler(SQSEvent evnt, ILambdaContext context)
    {
        //  throw new InvalidDataException();
        context.Logger.LogLine($"Inside the DocumentGenClientFromSQSHandler");
        foreach (var message in evnt.Records)
        {
            context.Logger.LogLine($"Processing - " + message.Body);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
            var response = client.Send(request);
            // Put this request in eventbridge
            await _eventBridgeRepository.PutEvent(message.Body, "arn:aws:events:ap-northeast-1:178515926936:event-bus/default");

        }

    }

    public async Task BoxClientFromSQSHandler(SQSEvent evnt, ILambdaContext context)
    {
        // throw new InvalidDataException();
        context.Logger.LogLine($"Inside the BoxClientFromSQSHandler");
        foreach (var message in evnt.Records)
        {
            context.Logger.LogLine($"Processing - " + message.Body);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
            var response = client.Send(request);
        }


    }

    public async Task BoxClientFromEventBridgeHandler(CloudWatchEvent<PolicyDetailsModel> eventBridgeRequest, ILambdaContext context)
    {
        // throw new InvalidDataException();
        context.Logger.LogLine($"Inside the BoxClientFromEventBridgeHandler");
        var policyDetails = _jsonConverter.SerializeObject(eventBridgeRequest.Detail);
        context.Logger.LogLine($"Processing - " + policyDetails);
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
        var response = client.Send(request);

    }

    #endregion

}
