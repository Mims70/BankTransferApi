namespace PaymentAPI.Models
{
    public class TransferRequest
    {
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
    }
}