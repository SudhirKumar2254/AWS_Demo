using AWSLambdas.Models;

namespace AWSLambdas.Dynamo
{
    public interface IEmployeeDetailsRepository
    {
        Task SaveEmployeeDetailsAsync(EmployeeDetailsModel employeeDetails);
    }
}
