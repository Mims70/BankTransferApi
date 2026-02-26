namespace PaymentAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}