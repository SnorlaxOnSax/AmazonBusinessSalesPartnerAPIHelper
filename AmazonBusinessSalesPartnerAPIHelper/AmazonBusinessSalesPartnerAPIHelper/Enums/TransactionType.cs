using System.ComponentModel;
using System.Runtime.Serialization;

namespace AmazonBusinessSalesPartnerAPIHelper.Enums
{
    public enum TransactionType
    {
        [EnumMember(Value = "CHARGE")]
        [Description("Charge")]
        Charge,
        [EnumMember(Value = "REFUND")]
        [Description("Refund")]
        Refund,
        [EnumMember(Value = "OVERREFUND")]
        [Description("Over Refund")]
        OverRefund,
    }
}