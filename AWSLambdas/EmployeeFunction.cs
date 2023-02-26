using Amazon.Lambda.Core;
using AWSLambdas.Models;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal.Util;
using AWSLambdas.Services;
using System.Net;
using AWSLambdas.Dynamo;
using Amazon.DynamoDBv2.Model;
using Microsoft.VisualBasic;
using Amazon.StepFunctions.Model;
using AWSLambdas.StepFunctions;
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

    public EmployeeFunction() : this(null, null, null, null, null) { }

    public EmployeeFunction(IJsonConverter jsonConverter, IDynamoDbClient dynamoDbClient, IEmployeeMessagesRepository employeeMessagesRepository, IStepFunctionsClient stepFunctionsClient, IStepFunctionsRepository stepFunctionsRepository)
    {
        _jsonConverter = jsonConverter ?? new JsonConverter();
        _dynamoDbClient = dynamoDbClient ?? new DynamoDbClient();
        _employeeMessagesRepository = employeeMessagesRepository ?? new EmployeeMessagesRepository(_dynamoDbClient);
        _stepFunctionsClient = stepFunctionsClient ?? new StepFunctionsClient();
        _stepFunctionsRepository = stepFunctionsRepository ?? new StepFunctionsRepository(_stepFunctionsClient);
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

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = _jsonConverter.SerializeObject(empDetails)
        };
    }

    public void PutMessageInDb(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.Log("Inside the PutMessageInDb");

        EmployeeMessageModel employeeMessage = new EmployeeMessageModel();
        employeeMessage.Message = request.Body;
        _employeeMessagesRepository.SaveEmployeeMessagesAsync(employeeMessage);

    }

    public async Task<APIGatewayProxyResponse> PostData(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.Log("Inside the PostData");
        var empDetails = _jsonConverter.DeserializeObject<EmployeeDetailsModel>(request.Body);

        var response = await _stepFunctionsRepository.StartExecution();

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = _jsonConverter.SerializeObject(empDetails)
        };
    }
}
