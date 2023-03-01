namespace AWSLambdas.Models
{
    public class PolicyDetailsModel
    {
        public string PolicyHolderName { get; set; }
        public DateTime PolicyStartDate { get; set; }
        public DateTime PolicyEndDate { get; set; }
        public int PolicyId { get; set; }

        public int Id { get; set; }

    }
}
