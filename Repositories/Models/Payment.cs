using System;

namespace Repositories.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Created { get; set; }
        public PaymentStatus PaymentType { get; set; }
        public Order Order { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Canceled
    }
}
