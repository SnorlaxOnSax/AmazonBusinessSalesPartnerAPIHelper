namespace AmazonBusinessSalesPartnerAPIHelper
{
    public class Transaction
    {
        public string MarketplaceId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? FeedDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionId { get; set; }
        public string ShipTaxRate { get; set; }
        public string GiftWrapTaxRate { get; set; }
        public string LegalEntityName { get; set; }
        public string PurchasingCustomerGroupName { get; set; }
        public Money Amount { get; set; }
        public string PaymentInstrumentType { get; set; }
        public string PaymentInstrumentLast4Digits { get; set; }
        public Customer Buyer { get; set; }
        public Customer Payer { get; set; }
        public List<AssociatedTransactionDetail> AssociatedTransactionDetails { get; set; }
        public List<TransactionLineItem> TransactionLineItems { get; set; }
    }
}