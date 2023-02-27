using AWSLambdas.Models;

namespace AWSLambdas.Dynamo
{
    public interface IPolicyDetailsRepository
    {
        Task SaveEmployeeDetailsAsync(EmployeeDetailsModel employeeDetails);
    }
}
