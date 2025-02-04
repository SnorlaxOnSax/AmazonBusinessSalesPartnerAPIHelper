using Newtonsoft.Json;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class OrderLineItem
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }
        [JsonProperty("orderLineItemId")]
        public string OrderLineItemId { get; set; }
        [JsonProperty("shipmentId")]
        public string ShipmentId { get; set; }
    }
}