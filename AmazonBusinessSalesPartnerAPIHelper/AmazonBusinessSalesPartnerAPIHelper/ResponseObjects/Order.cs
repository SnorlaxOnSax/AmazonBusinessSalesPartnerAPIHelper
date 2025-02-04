using AmazonBusinessSalesPartnerAPIHelper.Enums;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class Order
    {
        public string OrderDate { get; set; }
        public string OrderId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int OrderQuantity { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public string LastOrderApproverName { get; set; }
        public Customer BuyingCustomer { get; set; }
        public string BuyerGroupName { get; set; }
        public Dictionary<string,string> BusinessOrderInfo { get; set; }
        public Money OrderSubTotal { get; set; }
        public Money OrderShippingAndHandling { get; set; }
        public Money OrderPromotion { get; set; }
        public Money OrderTax { get; set; }
        public Money OrderNetTotal { get; set; }
        public IEnumerable<LineItem> LineItems { get; set; }
        public IEnumerable<Shipment> Shipments { get; set; }
        public IEnumerable<Charge> Charges { get; set; }
    }
}