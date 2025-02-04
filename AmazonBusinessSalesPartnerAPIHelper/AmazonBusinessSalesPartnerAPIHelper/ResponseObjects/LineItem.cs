namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class LineItem
    {
        public string ProductCategory { get; set; }
        public string ASIN { get; set; }
        public string Title { get; set; }
        public string UNSPSC { get; set; }
        public string ProductCondition { get; set; }
        public Money ListedPricePerUnit { get; set; }
        public Money PurchasedPricePerUnit { get; set; }
        public int ItemQuantity { get; set; }
        public Money ItemSubTotal { get; set; }
        public Money ItemShippingAndHandling { get; set; }
        public Money ItemPromotion { get; set; }
        public Money ItemTax { get; set; }
        public Money ItemNetTotal { get; set; }
        public string PurchaseOrderLineItem { get; set; }
        public bool? TaxExemptionApplied { get; set; }
        public string TaxExemptionType { get; set; }
        public bool? TaxExemptOptOut { get; set; }
        public string DiscountProgram { get; set; }
        public string DiscountType { get; set; }
        public Money DiscountAmount { get; set; }
        public string DiscountRatio { get; set; }
        public Seller Seller { get; set; }
        public IEnumerable<string> SellerCredentials { get; set; }
        public string BrandCode { get; set; }
        public string BrandName { get; set; }
        public string ManufacturerName { get; set; }
        public IEnumerable<string> TransactionIds { get; set; }
        public IEnumerable<string> CarrierTrackingNumbers { get; set; }
    }  
}