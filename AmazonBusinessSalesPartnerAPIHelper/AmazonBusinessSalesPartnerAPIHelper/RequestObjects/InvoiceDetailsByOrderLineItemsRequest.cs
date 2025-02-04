using AmazonBusinessSalesPartnerAPIHelper.ResponseObjects;
using Newtonsoft.Json;

namespace AmazonBusinessSalesPartnerAPIHelper.RequestObjects
{
    [JsonObject("invoiceDetailsByOrderLineItemsRequest")]
    public class InvoiceDetailsByOrderLineItemsRequest
    {
        [JsonProperty("orderLineItems")]
        public List<OrderLineItem> OrderLineItems { get; set; }
    }
}