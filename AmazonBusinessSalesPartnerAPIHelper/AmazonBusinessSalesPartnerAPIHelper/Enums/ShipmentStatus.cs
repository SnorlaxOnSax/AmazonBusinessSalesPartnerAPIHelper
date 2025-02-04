using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AmazonBusinessSalesPartnerAPIHelper.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShipmentStatus
    {
        [EnumMember(Value = "SHIPPED")]
        [Description("Shipped")]
        Shipped
    }
}