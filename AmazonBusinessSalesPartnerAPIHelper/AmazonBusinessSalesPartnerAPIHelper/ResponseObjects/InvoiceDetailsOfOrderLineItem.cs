namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class InvoiceDetailsOfOrderLineItem
    {
        public OrderLineItem OrderLineItem { get; set; }
        public List<InvoiceDetail> InvoiceDetails { get; set; }
    }
}