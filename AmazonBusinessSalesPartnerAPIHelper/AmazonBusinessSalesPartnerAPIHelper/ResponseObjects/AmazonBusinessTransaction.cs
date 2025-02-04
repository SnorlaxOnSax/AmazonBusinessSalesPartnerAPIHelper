namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    /// <summary>
    /// The data-transfer object - a class that has fields from the AB API mapped to it, this is a small selection of the possibilities
    /// </summary>
    public class AmazonBusinessTransaction
    {
        public DateTime? TransactionDate { get; set; }
        public DateTime? InvoiceIssueDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderId { get; set; }
        public string OrderLineItemId { get; set; }
        public string ShipmentId { get; set; }
        public string BusinessOrderInfo { get; set; }
        public string AmazonGroupName { get; set; }
        public string PurchaseTitle { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PurchasedBy { get; set; }
        public int Quantity { get; set; }
        public string UNSPSC { get; set; }
        
        //and so on...
    }
}