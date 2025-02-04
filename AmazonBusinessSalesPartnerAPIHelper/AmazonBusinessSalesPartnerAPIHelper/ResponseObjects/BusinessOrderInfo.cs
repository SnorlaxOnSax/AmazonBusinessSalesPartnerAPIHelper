using Newtonsoft.Json;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class BusinessOrderInfo
    {
        [JsonProperty("Who is this purchase for?")]
        public string WhoIsThisPurchaseFor { get; set; }
        [JsonProperty("Campus org code")]
        public string CampusOrgCode { get; set; }
        [JsonProperty("Non-campus org code")]
        public string NonCampusOrgCode { get; set; }
    }
}