using Newtonsoft.Json;

namespace AmazonBusinessSalesPartnerAPIHelper.RequestObjects
{
    [JsonObject("restrictedResources")]
    public class GetRDTTokenRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
    }
}