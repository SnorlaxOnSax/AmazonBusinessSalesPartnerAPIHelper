using System.Text;
using RestSharp;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    /// <summary>
    /// Adapted from https://github.com/amzn/selling-partner-api-models
    /// </summary>
    public sealed class AWSSigV4Signer
    {
        private readonly AWSAuthenticationCredentials awsCredentials;

        /// <summary>
        /// Constructor for AWSSigV4Signer
        /// </summary>
        /// <param name="awsAuthenticationCredentials">AWS Developer Account Credentials</param>
        public AWSSigV4Signer(AWSAuthenticationCredentials awsAuthenticationCredentials)
        {
            awsCredentials = awsAuthenticationCredentials;
        }

        /// <summary>
        /// Signs a Request with AWS Signature Version 4
        /// </summary>
        /// <param name="request">RestRequest which needs to be signed</param>
        /// <param name="host">Request endpoint</param>
        /// <returns>RestRequest with AWS Signature</returns>
        public RestRequest Sign(RestRequest request, string host)
        {
            var signingDate = AWSSignerHelper.InitializeHeaders(request, host);
            var signedHeaders = AWSSignerHelper.ExtractSignedHeaders(request);

            var hashedCanonicalRequest = CreateCanonicalRequest(request, signedHeaders);

            var stringToSign = AWSSignerHelper.BuildStringToSign(signingDate,
                                                                    hashedCanonicalRequest,
                                                                    awsCredentials.Region);

            var signature = AWSSignerHelper.CalculateSignature(stringToSign,
                                                                  signingDate,
                                                                  awsCredentials.SecretKey,
                                                                  awsCredentials.Region);

            AWSSignerHelper.AddSignature(request,
                                         awsCredentials.AccessKeyId,
                                         signedHeaders,
                                         signature,
                                         awsCredentials.Region,
                                         signingDate);

            return request;
        }

        private string CreateCanonicalRequest(RestRequest restRequest, string signedHeaders)
        {
            var canonicalizedRequest = new StringBuilder();
            //Request Method
            canonicalizedRequest.AppendFormat("{0}\n", restRequest.Method.ToString().ToUpper());

            //CanonicalURI
            canonicalizedRequest.AppendFormat("{0}\n", AWSSignerHelper.ExtractCanonicalURIParameters(restRequest));

            //CanonicalQueryString
            canonicalizedRequest.AppendFormat("{0}\n", AWSSignerHelper.ExtractCanonicalQueryString(restRequest));

            //CanonicalHeaders
            canonicalizedRequest.AppendFormat("{0}\n", AWSSignerHelper.ExtractCanonicalHeaders(restRequest));

            //SignedHeaders
            canonicalizedRequest.AppendFormat("{0}\n", signedHeaders);

            // Hash(digest) the payload in the body
            canonicalizedRequest.AppendFormat(AWSSignerHelper.HashRequestBody(restRequest));

            var canonicalRequest = canonicalizedRequest.ToString();

            //Create a digest(hash) of the canonical request
            return Utils.ToHex(Utils.Hash(canonicalRequest));
        }
    }
}