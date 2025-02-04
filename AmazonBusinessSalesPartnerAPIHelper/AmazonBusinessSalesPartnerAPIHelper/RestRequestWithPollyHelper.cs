using Polly;
using RestSharp;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    /// <summary>
    /// Helper class implementing retry policies for RestRequests with Polly
    /// </summary>
    public static class RestRequestWithPollyHelper
    {
        /// <summary>
        /// Executes a RestClient request with wait and retry logic
        /// </summary>
        /// <param name="restClient"></param>
        /// <param name="restRequest"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public static RestResponse ExecuteRestRequestWithWaitAndRetry(RestClient restClient, RestRequest restRequest, int retryCount = 6)
        {
            var jitter = new Random();

            var retryPolicy = Policy
                .HandleResult<RestResponse>(restResponse => !restResponse.IsSuccessful)
                .WaitAndRetry(retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
                                                      + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)));

            return retryPolicy.Execute(() => restClient.Execute(restRequest));
        }
    }
}