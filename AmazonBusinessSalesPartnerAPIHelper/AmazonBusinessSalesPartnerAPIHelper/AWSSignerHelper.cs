using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    /// <summary>
    /// Adapted from https://github.com/amzn/selling-partner-api-models
    /// </summary>
    public static class AWSSignerHelper
    {
        private const string ISO8601BasicDateTimeFormat = "yyyyMMddTHHmmssZ";
        private const string ISO8601BasicDateFormat = "yyyyMMdd";

        private const string XAmzDateHeaderName = "x-amz-date";
        private const string AuthorizationHeaderName = "Authorization";
        private const string CredentialSubHeaderName = "Credential";
        private const string SignatureSubHeaderName = "Signature";
        private const string SignedHeadersSubHeaderName = "SignedHeaders";
        private const string HostHeaderName = "host";

        private const string Scheme = "AWS4";
        private const string Algorithm = "HMAC-SHA256";
        private const string TerminationString = "aws4_request";
        private const string ServiceName = "execute-api";
        private const string Slash = "/";

        private static readonly Regex CompressWhitespaceRegex = new Regex("\\s+");
        
        /// <summary>
        /// Returns URI encoded version of absolute path
        /// </summary>
        /// <param name="request">RestRequest</param>
        /// <returns>URI encoded version of absolute path</returns>
        public static string ExtractCanonicalURIParameters(RestRequest request)
        {
            var resource = request.Resource;
            var canonicalUri = string.Empty;

            if (string.IsNullOrEmpty(resource))
            {
                canonicalUri = Slash;
            }
            else
            {
                if (!resource.StartsWith(Slash))
                {
                    canonicalUri = Slash;
                }
                IDictionary<string, string> pathParameters = request.Parameters
                        .Where(parameter => ParameterType.UrlSegment.Equals(parameter.Type))
                        .ToDictionary(parameter => parameter.Name?.Trim().ToString(), parameter => parameter.Value?.ToString());

                // Replace path parameter with actual value.
                // Ex: /products/pricing/v0/items/{Asin}/offers -> /products/pricing/v0/items/AB12CD3E4Z/offers
                resource = pathParameters.Keys.Aggregate(resource, (current, parameter) => current.Replace("{" + parameter + "}", pathParameters[parameter]));

                //Split path at / into segments
                IEnumerable<string> encodedSegments = resource.Split(new[] { '/' }, StringSplitOptions.None);

                // Encode twice
                encodedSegments = encodedSegments.Select(Utils.UrlEncode);
                encodedSegments = encodedSegments.Select(Utils.UrlEncode);

                canonicalUri += string.Join(Slash, encodedSegments.ToArray());
            }

            return canonicalUri;
        }

        /// <summary>
        /// Returns query parameters in canonical order with URL encoding
        /// </summary>
        /// <param name="request">RestRequest</param>
        /// <returns>Query parameters in canonical order with URL encoding</returns>
        public static string ExtractCanonicalQueryString(RestRequest request)
        {
            IDictionary<string, string> queryParameters = request.Parameters
                .Where(parameter => ParameterType.QueryString.Equals(parameter.Type))
                .ToDictionary(parameter => parameter.Name?.Trim().ToString(), parameter => parameter.Value?.ToString());

            var sortedQueryParameters = new SortedDictionary<string, string>(queryParameters);

            var canonicalQueryString = new StringBuilder();
            foreach (var key in sortedQueryParameters.Keys)
            {
                if (canonicalQueryString.Length > 0)
                {
                    canonicalQueryString.Append("&");
                }
                canonicalQueryString.AppendFormat("{0}={1}",
                    Utils.UrlEncode(key),
                    Utils.UrlEncode(sortedQueryParameters[key]));
            }

            return canonicalQueryString.ToString();
        }

        /// <summary>
        /// Returns Http headers in canonical order with all header names to lowercase
        /// </summary>
        /// <param name="request">RestRequest</param>
        /// <returns>Returns Http headers in canonical order</returns>
        public static string ExtractCanonicalHeaders(RestRequest request)
        {
            IDictionary<string, string> headers = request.Parameters
                .Where(parameter => ParameterType.HttpHeader.Equals(parameter.Type))
                .ToDictionary(header => header.Name?.Trim().ToLowerInvariant(), header => header.Value?.ToString());

            var sortedHeaders = new SortedDictionary<string, string>(headers);

            var headerString = new StringBuilder();

            foreach (var headerName in sortedHeaders.Keys)
            {
                headerString.AppendFormat("{0}:{1}\n",
                    headerName,
                    CompressWhitespaceRegex.Replace(sortedHeaders[headerName].Trim(), " "));
            }

            return headerString.ToString();
        }

        /// <summary>
        /// Returns list(as string) of Http headers in canonical order
        /// </summary>
        /// <param name="request">RestRequest</param>
        /// <returns>List of Http headers in canonical order</returns>
        public static string ExtractSignedHeaders(RestRequest request)
        {
            var rawHeaders = request.Parameters.Where(parameter => ParameterType.HttpHeader.Equals(parameter.Type))
                                                        .Select(header => header.Name?.Trim().ToLowerInvariant())
                                                        .ToList();
            rawHeaders.Sort(StringComparer.OrdinalIgnoreCase);

            return string.Join(";", rawHeaders);
        }

        /// <summary>
        /// Returns hexadecimal hashed value(using SHA256) of payload in the body of request
        /// </summary>
        /// <param name="request">RestRequest</param>
        /// <returns>Hexadecimal hashed value of payload in the body of request</returns>
        public static string HashRequestBody(RestRequest request)
        {
            var body = request.Parameters.FirstOrDefault(parameter => ParameterType.RequestBody.Equals(parameter.Type));
            var value = body != null ? JsonConvert.SerializeObject(body.Value) : string.Empty;
            return Utils.ToHex(Utils.Hash(value));
        }

        /// <summary>
        /// Builds the string for signing using signing date, hashed canonical request and region
        /// </summary>
        /// <param name="signingDate">Signing Date</param>
        /// <param name="hashedCanonicalRequest">Hashed Canonical Request</param>
        /// <param name="region">Region</param>
        /// <returns>String to be used for signing</returns>
        public static string BuildStringToSign(DateTime signingDate, string hashedCanonicalRequest, string region)
        {
            var scope = BuildScope(signingDate, region);
            var stringToSign = string.Format(CultureInfo.InvariantCulture, "{0}-{1}\n{2}\n{3}\n{4}",
                Scheme,
                Algorithm,
                signingDate.ToString(ISO8601BasicDateTimeFormat, CultureInfo.InvariantCulture),
                scope,
                hashedCanonicalRequest);

            return stringToSign;
        }

        /// <summary>
        /// Sets AWS4 mandated 'x-amz-date' header, returning the date/time that will
        /// be used throughout the signing process.
        /// </summary>
        /// <param name="restRequest">RestRequest</param>
        /// <param name="host">Request endpoint</param>
        /// <returns>Date and time used for x-amz-date, in UTC</returns>
        public static DateTime InitializeHeaders(RestRequest restRequest, string host)
        {
            restRequest.Parameters.RemoveParameter(XAmzDateHeaderName);
            restRequest.Parameters.RemoveParameter(HostHeaderName);

            var signingDate = DateTime.UtcNow;

            restRequest.AddHeader(XAmzDateHeaderName, signingDate.ToString(ISO8601BasicDateTimeFormat, CultureInfo.InvariantCulture));
            restRequest.AddHeader(HostHeaderName, host);

            return signingDate;
        }

        /// <summary>
        /// Calculates AWS4 signature for the string, prepared for signing
        /// </summary>
        /// <param name="stringToSign">String to be signed</param>
        /// <param name="signingDate">Signing Date</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="region">Region</param>
        /// <returns>AWS4 Signature</returns>
        public static string CalculateSignature(string stringToSign,
                                                 DateTime signingDate,
                                                 string secretKey,
                                                 string region)
        {
            var date = signingDate.ToString(ISO8601BasicDateFormat, CultureInfo.InvariantCulture);
            var kSecret = Encoding.UTF8.GetBytes((Scheme + secretKey).ToCharArray());
            var kDate = Utils.GetKeyedHash(kSecret, date);
            var kRegion = Utils.GetKeyedHash(kDate, region);
            var kService = Utils.GetKeyedHash(kRegion, ServiceName);
            var kSigning = Utils.GetKeyedHash(kService, TerminationString);

            // Calculate the signature
            return Utils.ToHex(Utils.GetKeyedHash(kSigning, stringToSign));
        }

        /// <summary>
        /// Add a signature to a request in the form of an 'Authorization' header
        /// </summary>
        /// <param name="restRequest">Request to be signed</param>
        /// <param name="accessKeyId">Access Key Id</param>
        /// <param name="signedHeaders">Signed Headers</param>
        /// <param name="signature">The signature to add</param>
        /// <param name="region">AWS region for the request</param>
        /// <param name="signingDate">Signature date</param>
        public static void AddSignature(RestRequest restRequest,
                                         string accessKeyId,
                                         string signedHeaders,
                                         string signature,
                                         string region,
                                         DateTime signingDate)
        {
            var scope = BuildScope(signingDate, region);
            var authorizationHeaderValueBuilder = new StringBuilder();
            authorizationHeaderValueBuilder.AppendFormat("{0}-{1}", Scheme, Algorithm);
            authorizationHeaderValueBuilder.AppendFormat(" {0}={1}/{2},", CredentialSubHeaderName, accessKeyId, scope);
            authorizationHeaderValueBuilder.AppendFormat(" {0}={1},", SignedHeadersSubHeaderName, signedHeaders);
            authorizationHeaderValueBuilder.AppendFormat(" {0}={1}", SignatureSubHeaderName, signature);

            restRequest.AddHeader(AuthorizationHeaderName, authorizationHeaderValueBuilder.ToString());
        }

        private static string BuildScope(DateTime signingDate, string region)
        {
            return
                $"{signingDate.ToString(ISO8601BasicDateFormat, CultureInfo.InvariantCulture)}/{region}/{ServiceName}/{TerminationString}";
        }
    }
}
