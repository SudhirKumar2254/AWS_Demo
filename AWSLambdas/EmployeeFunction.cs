using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdas.Dynamo;
using AWSLambdas.Models;
using AWSLambdas.Services;
using AWSLambdas.SNS;
using AWSLambdas.StepFunctions;
using System.Net;
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

    public EmployeeFunction() : this(null, null, null, null, null, null, null) { }

    public EmployeeFunction(IJsonConverter jsonConverter, IDynamoDbClient dynamoDbClient, IEmployeeMessagesRepository employeeMessagesRepository, IStepFunctionsClient stepFunctionsClient, IStepFunctionsRepository stepFunctionsRepository, ISnsClient snsClient, ISnsRepository snsRepository)
    {
        _jsonConverter = jsonConverter ?? new JsonConverter();
        _dynamoDbClient = dynamoDbClient ?? new DynamoDbClient();
        _employeeMessagesRepository = employeeMessagesRepository ?? new EmployeeMessagesRepository(_dynamoDbClient);
        _stepFunctionsClient = stepFunctionsClient ?? new StepFunctionsClient();
        _stepFunctionsRepository = stepFunctionsRepository ?? new StepFunctionsRepository(_stepFunctionsClient);
        _snsClient = snsClient ?? new SnsClient();
        _snsRepository = snsRepository ?? new SnsRepository(_snsClient);
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
        context.Logger.Log("Inside the PutMessageInDb");

        EmployeeMessageModel employeeMessage = new EmployeeMessageModel();
        employeeMessage.Message = request.Body;
        _employeeMessagesRepository.SaveEmployeeMessagesAsync(employeeMessage);

    }

    public void SendSNSNotification(ILambdaContext context, string emailSubject, string emailBody)
    {
        context.Logger.Log("Inside SendSNSNotification");
        _snsRepository.SendNotification(emailSubject,emailBody);

    }

    public async Task<APIGatewayProxyResponse> PostData(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.Log("Inside the PostData");
        var empDetails = _jsonConverter.DeserializeObject<EmployeeDetailsModel>(request.Body);

        var response = await _stepFunctionsRepository.StartExecution();
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


}
