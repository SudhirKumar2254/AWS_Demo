using Amazon.Lambda.Core;
using Amazon.Runtime;

namespace AWSLambdas;

public class StepFunctionTasks
{
    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public StepFunctionTasks()
    {
    }

    public State PostDataToExternalApi1(State state, ILambdaContext context)
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
        var response = client.Send(request);

        state.Message = response.Content.ReadAsStringAsync().Result;
        if (!string.IsNullOrEmpty(state.Name))
        {
            state.Message += " " + state.Name;
        }

        // Tell Step Function to wait 5 seconds before calling 
        state.WaitInSeconds = 5;

        return state;
    }

    public State PostDataToExternalApi2(State state, ILambdaContext context)
    {
        state.Message += ", Goodbye";

        if (!string.IsNullOrEmpty(state.Name))
        {
            state.Message += " " + state.Name;
        }

        return state;
    }

    public State DocumentGenClientHandler(State state, ILambdaContext context)
    {
      //  throw new InvalidDataException();
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
        var response = client.Send(request);

        state.Message = response.Content.ReadAsStringAsync().Result;
        if (!string.IsNullOrEmpty(state.Name))
        {
            state.Message += " " + state.Name;
        }

        Thread.Sleep(10000);
        // Tell Step Function to wait 5 seconds before calling 
        state.WaitInSeconds = 5;

        return state;
    }

    public State BoxClientHandler(State state, ILambdaContext context)
    {
         // throw new InvalidDataException();
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://random-data-api.com/api/beer/random_beer");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
        var response = client.Send(request);

        state.Message = response.Content.ReadAsStringAsync().Result;
        if (!string.IsNullOrEmpty(state.Name))
        {
            state.Message += " " + state.Name;
        }

        Thread.Sleep(10000);
        // Tell Step Function to wait 5 seconds before calling 
        state.WaitInSeconds = 5;

        return state;
    }
}