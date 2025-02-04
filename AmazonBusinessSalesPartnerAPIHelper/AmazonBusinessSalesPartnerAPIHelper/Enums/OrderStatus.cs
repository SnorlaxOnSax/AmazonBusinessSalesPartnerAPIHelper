using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AmazonBusinessSalesPartnerAPIHelper.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        [EnumMember(Value = "PENDING_APPROVAL")]
        [Description("Pending Approval")]
        PendingApproval,
        [EnumMember(Value = "PAYMENT_CONFIRMED")]
        [Description("Pending Approval")]
        PaymentConfirmed,
        [EnumMember(Value = "PENDING_FULFILLMENT")]
        [Description("Pending Fulfillment")]
        PendingFulfillment,
        [EnumMember(Value = "PENDING")]
        [Description("Pending")]
        Pending,
        [EnumMember(Value = "CANCELLED")]
        [Description("Cancelled")]
        Cancelled,
        [EnumMember(Value = "CLOSED")]
        [Description("Closed")]
        Closed
    }
}