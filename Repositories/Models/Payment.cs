namespace Repositories.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public DateTime Created { get; set; }
        public PaymentType PaymentType { get; set; }
        public Order Order { get; set; }
    }

    public enum PaymentType
    {
        Pending,
        Completed,
        Canceled
    }
}
