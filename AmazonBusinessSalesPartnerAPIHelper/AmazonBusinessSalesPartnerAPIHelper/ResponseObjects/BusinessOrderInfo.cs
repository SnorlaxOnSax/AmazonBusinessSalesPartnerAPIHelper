using Newtonsoft.Json;

namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    /// <summary>
    /// BusinessOrderInfo.cs is a great example of a class that needed to be made to handle some custom fields (specific to my client) coming back from the API response.
    /// On the situation that you need to integrate with custom fields, this pattern should help.
    /// </summary>
    public class BusinessOrderInfo
    {
        [JsonProperty("Name of property case sensitive")]
        public string NameOfPropertyCaseSensitive { get; set; }

        // ...and so on
    }
}