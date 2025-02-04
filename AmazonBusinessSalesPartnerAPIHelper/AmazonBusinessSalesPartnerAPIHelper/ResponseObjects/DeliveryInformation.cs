using AmazonBusinessSalesPartnerAPIHelper.Enums;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class DeliveryInformation
    {
        public string ExpectedDeliveryDate { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
    }
}