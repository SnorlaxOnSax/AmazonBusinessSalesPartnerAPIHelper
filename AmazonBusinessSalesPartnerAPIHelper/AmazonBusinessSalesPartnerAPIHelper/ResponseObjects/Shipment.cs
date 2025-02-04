using AmazonBusinessSalesPartnerAPIHelper.Enums;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class Shipment
    {
        public DateTime ShipmentDate { get; set; }
        public ShipmentStatus? ShipmentStatus { get; set; }
        public string CarrierTracking { get; set; }
        public DeliveryInformation DeliveryInformation { get; set; }
        public int ShipmentQuantity { get; set; }
        public Money ShipmentSubTotal { get; set; }
        public Money ShipmentShippingAndHandling { get; set; }
        public Money ShipmentPromotion { get; set; }
        public Money ShipmentTax { get; set; }
        public Money ShipmentNetTotal { get; set; }
        public string CarrierName { get; set; }
    }
}