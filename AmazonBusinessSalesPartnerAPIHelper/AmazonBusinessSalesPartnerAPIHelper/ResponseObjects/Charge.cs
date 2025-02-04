namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class Charge
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; }
        public Money Amount { get; set; }
        public string PaymentInstrumentType { get; set; }
        public string PaymentInstrumentLast4Digits { get; set; }
    }
}