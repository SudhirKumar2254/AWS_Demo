using AWSLambdas.Models;

namespace AWSLambdas.Dynamo
{
    public interface IEmployeeMessagesRepository
    {
        Task SaveEmployeeMessagesAsync(EmployeeMessageModel employeeMessage);
    }
}
