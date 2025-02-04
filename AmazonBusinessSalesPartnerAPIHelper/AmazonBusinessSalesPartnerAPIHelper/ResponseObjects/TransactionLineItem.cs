namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class TransactionLineItem
    {
        public string RefundReason { get; set; }
        public Money TotalAmount { get; set; }
        public Money PrincipalAmount { get; set; }
        public Money ShippingCharge { get; set; }
        public Money RegulatoryFee { get; set; }
        public Money GiftWrappingCharge { get; set; }
        public Money Discount { get; set; }
        public Money Tax { get; set; }
        public Money UnitPrice { get; set; }
        public string TaxRate { get; set; }
        public string OrderId { get; set; }
        public string UNSPSC { get; set; }
        public string OrderLineItemId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PurchaseOrderLineItemId { get; set; }
        public string ASIN { get; set; }
        public string ProductTitle { get; set; }
        public int ItemQuantity { get; set; }
        public string ShipmentId { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public string MerchantLegalName { get; set; }
        public string BudgetId { get; set; }
        public Dictionary<string, string> BusinessOrderInfo { get; set; }
    }
}