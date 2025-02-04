using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AmazonBusinessSalesPartnerAPIHelper.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryStatus
    {
        [EnumMember(Value = "DELIVERED")]
        [Description("Delivered")]
        Delivered,
        [EnumMember(Value = "NOT_DELIVERED")]
        [Description("Not Delivered")]
        NotDelivered,
        [EnumMember(Value = "NOT_AVAILABLE")]
        [Description("Not Available")]
        NotAvailable
    }
}