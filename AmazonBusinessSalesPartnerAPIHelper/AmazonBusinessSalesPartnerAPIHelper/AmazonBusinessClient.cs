using Microsoft.Extensions.Configuration;
using RestSharp;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    /// <summary>
    /// Client to handle Amazon API integration.
    /// All requests to Amazon APIs will require an LWA Token and AWS V4 signature information,
    /// for new requests be sure to call GetLWAToken() and use AWSSigV4Signer 
    /// </summary>
    public class AmazonBusinessClient : IAmazonBusinessClient
    {
        private readonly AWSSigV4Signer _signatureSigner;
        
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;
        private readonly string _baseUrl;
        private readonly string _authorizationBaseUrl;
        private readonly string _abOrdersGetOrderByOrderIdEndpoint;
        private readonly string _abReconciliationGetTransactionsEndpoint;
        private readonly string _abReconciliationGetInvoiceDetailsEndpoint;

        private const string ISO8601BasicDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string PayByInvoice = "Pay by Invoice";
        private const string TrimmedBaseUrlForSigning = "na.business-api.amazon.com";

        public AmazonBusinessClient(IConfiguration config)
        {
            _clientId = config.GetSection("AmazonBusiness").GetValue<string>("ClientId");
            _clientSecret = config.GetSection("AmazonBusiness").GetValue<string>("ClientSecret");
            _refreshToken = config.GetSection("AmazonBusiness").GetValue<string>("RefreshToken");
            _baseUrl = config.GetSection("AmazonBusiness").GetValue<string>("BaseApiUrl");
            _authorizationBaseUrl = config.GetSection("AmazonBusiness").GetValue<string>("AuthorizationBaseApiUrl");
            config.GetSection("AmazonBusiness").GetValue<int>("DaysToQuery");
            _abOrdersGetOrderByOrderIdEndpoint = config.GetSection("AmazonBusiness").GetValue<string>("ABOrdersGetOrderByOrderIdEndpoint");
            _abReconciliationGetTransactionsEndpoint = config.GetSection("AmazonBusiness").GetValue<string>("ABReconciliationGetTransactionsEndpoint");
            _abReconciliationGetInvoiceDetailsEndpoint = config.GetSection("AmazonBusiness").GetValue<string>("ABReconciliationGetInvoiceDetailsEndpoint");
            
            _signatureSigner = new AWSSigV4Signer(new AWSAuthenticationCredentials
            {
                AccessKeyId = config.GetSection("AmazonBusiness").GetValue<string>("AWSAccessKeyId"),
                SecretKey = config.GetSection("AmazonBusiness").GetValue<string>("AWSSecretAccessKey"),
                Region = config.GetSection("AmazonBusiness").GetValue<string>("AWSRegion")
            });
        }

        /// <summary>
        /// Calls Amazon API to return the LWA (Login With Amazon) token that's required for all requests
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public string GetLWAToken()
        {
            var lwaClient = new RestClient(_authorizationBaseUrl);
            var lwaTokenRequestBody = new LWATokenRequestBody
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RefreshToken = _refreshToken,
                GrantType = "refresh_token"
            };
            var lwaTokenRequest = new RestRequest("auth/O2/token", Method.Post);

            lwaTokenRequest.AddHeader("Accept", "*/*");
            lwaTokenRequest.AddHeader("Content-Type", "application/json");
            lwaTokenRequest.AddHeader("Connection", "keep-alive");
            lwaTokenRequest.AddBody(lwaTokenRequestBody);

            string lwaToken;
            try
            {
                var response = lwaClient.Execute(lwaTokenRequest);
                if (response.ResponseStatus == ResponseStatus.Completed && response.Content != null)
                {
                    var responseJson = JObject.Parse(response.Content);
                    lwaToken = responseJson.GetValue("access_token")?.ToString();
                }
                else
                {
                    throw new IOException("LWA token request failed", response.ErrorException);
                }
            }
            catch (Exception)
            {
                throw new HttpRequestException();
            }

            return lwaToken;
        }

        /// <summary>
        /// Calls Amazon Business Reconciliation API to get transactions for a given time range
        /// </summary>
        /// <param name="lwaToken"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public List<AmazonBusinessTransaction> GetAmazonBusinessTransactions(string lwaToken, DateTime startDate, DateTime endDate)
        {
            List<AmazonBusinessTransaction> mappedTransactions;
            using var client = new RestClient(_baseUrl);
            try
            {
                var request = new RestRequest(_abReconciliationGetTransactionsEndpoint);
                request.AddHeader("x-amz-access-token", lwaToken);
                request.AddQueryParameter("feedStartDate", startDate.ToString(ISO8601BasicDateTimeFormat));
                request.AddQueryParameter("feedEndDate", endDate.ToString(ISO8601BasicDateTimeFormat));
                request = _signatureSigner.Sign(request, TrimmedBaseUrlForSigning);

                //execute API call for transactions
                var rawTransactions = new List<Transaction>();
                var response = client.Execute(request);
                if (response.ResponseStatus == ResponseStatus.Completed && response.Content != null)
                {
                    var transactionsResponse = JsonConvert.DeserializeObject<TransactionsResponse>(response.Content);
                    if (transactionsResponse == null) return null;

                    rawTransactions = transactionsResponse.Transactions.ToList();

                    //process nextPageToken if we continue to get one back in the response
                    var nextPageToken = transactionsResponse.NextPageToken;
                    while (!string.IsNullOrEmpty(nextPageToken))
                    {
                        request = new RestRequest(_abReconciliationGetTransactionsEndpoint);
                        request.AddHeader("x-amz-access-token", lwaToken);
                        request.AddQueryParameter("feedStartDate", startDate.ToString(ISO8601BasicDateTimeFormat));
                        request.AddQueryParameter("feedEndDate", endDate.ToString(ISO8601BasicDateTimeFormat));
                        request.AddQueryParameter("nextPageToken", nextPageToken);
                        request = _signatureSigner.Sign(request, TrimmedBaseUrlForSigning);

                        response = client.Execute(request);
                        if (response.ResponseStatus != ResponseStatus.Completed || response.Content == null) continue;

                        var nextPageTransactionsResponse = JsonConvert.DeserializeObject<TransactionsResponse>(response.Content);
                        if (nextPageTransactionsResponse == null) continue;

                        rawTransactions.AddRange(nextPageTransactionsResponse.Transactions.ToList());
                        nextPageToken = nextPageTransactionsResponse.NextPageToken;
                    }
                }
                
                if (!rawTransactions.Any()) return null;
                
                //filter out orders without invoice payments and only get orders where BusinessOrderInfo != null
                var filteredTxns = rawTransactions.Where(x => x.PaymentInstrumentType == PayByInvoice && x.TransactionLineItems.Any(b => b.BusinessOrderInfo != null)).ToList();

                //map initial data to AmazonBusinessTransactions
                mappedTransactions = MapAmazonBusinessTransactions(filteredTxns);
            }
            catch (Exception e)
            {
                throw new HttpRequestException("Request for data from Amazon Business failed.", e);
            }

            return mappedTransactions;
        }
        private static List<AmazonBusinessTransaction> MapAmazonBusinessTransactions(List<Transaction> transactions)
        {
            return (from txn in transactions
                from lineItem in txn.TransactionLineItems
                select new AmazonBusinessTransaction
                {
                    OrderId = lineItem.OrderId,
                    OrderLineItemId = lineItem.OrderLineItemId,
                    ShipmentId = lineItem.ShipmentId,
                    PurchaseTitle = lineItem.ProductTitle,
                    PaymentAmount = Convert.ToDecimal(lineItem.TotalAmount.Amount),
                    PurchasedBy = txn.Buyer.Name,
                    Quantity = lineItem.ItemQuantity,
                    UNSPSC = lineItem.UNSPSC,
                    BusinessOrderInfo = JsonConvert.SerializeObject(lineItem.BusinessOrderInfo),
                    AmazonGroupName = txn.PurchasingCustomerGroupName,
                    CreatedDate = DateTime.UtcNow,
                    TransactionDate = txn.TransactionDate != DateTime.MinValue ? txn.TransactionDate : null
                }).ToList();
        }

        /// <summary>
        /// Fetches invoice information from the Amazon Business Reconciliation API for the passed-in transactions.
        /// Sends a batched request with 25 transactions at a time to the API. 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="lwaToken"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public List<AmazonBusinessTransaction> GetInvoiceInformationForAmazonBusinessTransactions(List<AmazonBusinessTransaction> transactions, string lwaToken)
        {
            var transactionsWithInvoices = new List<AmazonBusinessTransaction>();
            using var client = new RestClient(_baseUrl);
            try
            {
                var batchCounter = 0;
                var batchedTransactions = new List<AmazonBusinessTransaction>();
                var skippedTransactions = new List<AmazonBusinessTransaction>();
                var batchedOrderLineItems = new List<OrderLineItem>();
                var finalTransaction = transactions.LastOrDefault();
                
                foreach (var txn in transactions)
                {
                    //build batch of transaction information for request
                    if (string.IsNullOrEmpty(txn.OrderId) || string.IsNullOrEmpty(txn.OrderLineItemId) ||
                        string.IsNullOrEmpty(txn.ShipmentId))
                    {
                        //handle transactions we don't have enough information for to send to Amazon 
                        skippedTransactions.Add(txn);
                        continue;
                    }
                    
                    batchedTransactions.Add(txn);
                    batchedOrderLineItems.Add(new OrderLineItem
                    {
                        OrderId = txn.OrderId,
                        OrderLineItemId = txn.OrderLineItemId,
                        ShipmentId = txn.ShipmentId
                    });
                    batchCounter++;
                    
                    //once there are 25 transactions in batch, build request
                    if (batchCounter < 25 && !txn.Equals(finalTransaction)) continue;
                    var requestObject = new InvoiceDetailsByOrderLineItemsRequest
                    {
                        OrderLineItems = batchedOrderLineItems
                    };
                    
                    var request = new RestRequest(_abReconciliationGetInvoiceDetailsEndpoint, Method.Post);
                    request.AddHeader("x-amz-access-token", lwaToken);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddBody(requestObject);
                    request = _signatureSigner.Sign(request, TrimmedBaseUrlForSigning);

                    //send request and process response
                    var response = RestRequestWithPollyHelper.ExecuteRestRequestWithWaitAndRetry(client, request);
                    if (response.ResponseStatus != ResponseStatus.Completed || response.Content == null)
                    {
                        throw new HttpRequestException();
                    }

                    var invoiceDetailsResponse = JsonConvert.DeserializeObject<InvoiceDetailsByOrderLineItemResponse>(response.Content);
                    if (invoiceDetailsResponse == null) 
                        continue;

                    //add data to returned set
                    foreach (var invoiceDetailsOfOrderLineItem in invoiceDetailsResponse.InvoiceDetailsByOrderLineItems)
                    {
                        var updatedTransaction = batchedTransactions.FirstOrDefault(bt =>
                            bt.OrderId == invoiceDetailsOfOrderLineItem.OrderLineItem.OrderId &&
                            bt.OrderLineItemId == invoiceDetailsOfOrderLineItem.OrderLineItem.OrderLineItemId &&
                            bt.ShipmentId == invoiceDetailsOfOrderLineItem.OrderLineItem.ShipmentId);
                        if (updatedTransaction == null)
                            continue;

                        foreach (var invoiceDetail in invoiceDetailsOfOrderLineItem.InvoiceDetails)
                        {
                            //if more than one invoice, create new transaction with each different invoice
                            updatedTransaction.InvoiceNumber = invoiceDetail.InvoiceNumber;
                            updatedTransaction.InvoiceIssueDate = invoiceDetail.InvoiceDate.GetValueOrDefault() != DateTime.MinValue ? invoiceDetail.InvoiceDate : null;
                            updatedTransaction.InvoiceDueDate = invoiceDetail.InvoiceDate.GetValueOrDefault() != DateTime.MinValue ? invoiceDetail.InvoiceDate.GetValueOrDefault().AddDays(45) : null;
                            transactionsWithInvoices.Add(updatedTransaction);
                        }
                    }

                    //reset batch variables
                    batchCounter = 0;
                    batchedTransactions = new List<AmazonBusinessTransaction>();
                    batchedOrderLineItems = new List<OrderLineItem>();
                }
                
                //add transactions we couldn't get invoice information for to the return list to be flagged
                transactionsWithInvoices.AddRange(skippedTransactions);
            }
            catch (Exception)
            {
                throw new HttpRequestException();
            }

            return transactionsWithInvoices;
        }

        public List<AmazonBusinessTransaction> CheckForUpdatedInvoiceInfo(string lwaToken, List<AmazonBusinessTransaction> transactionsWithoutInvoiceInformation)
        {
            var transactionsWithInvoiceInfo = new List<AmazonBusinessTransaction>();
            
            using var client = new RestClient(_baseUrl);
            try
            {
                var batchCounter = 0;
                var batchedTransactions = new List<AmazonBusinessTransaction>();
                var batchedOrderLineItems = new List<OrderLineItem>();
                var finalTransaction = transactionsWithoutInvoiceInformation.LastOrDefault();
                
                foreach (var txn in transactionsWithoutInvoiceInformation)
                {
                    //build batch of transaction information for request
                    if (string.IsNullOrEmpty(txn.OrderId) || string.IsNullOrEmpty(txn.OrderLineItemId) || string.IsNullOrEmpty(txn.ShipmentId))
                    {
                        continue;
                    }
                    
                    batchedTransactions.Add(txn);
                    batchedOrderLineItems.Add(new OrderLineItem
                    {
                        OrderId = txn.OrderId,
                        OrderLineItemId = txn.OrderLineItemId,
                        ShipmentId = txn.ShipmentId
                    });
                    batchCounter++;
                    
                    //once there are 25 transactions in batch, build request
                    if (batchCounter < 25 && !txn.Equals(finalTransaction)) continue;
                    var requestObject = new InvoiceDetailsByOrderLineItemsRequest
                    {
                        OrderLineItems = batchedOrderLineItems
                    };
                    
                    var request = new RestRequest(_abReconciliationGetInvoiceDetailsEndpoint, Method.Post);
                    request.AddHeader("x-amz-access-token", lwaToken);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddBody(requestObject);
                    request = _signatureSigner.Sign(request, TrimmedBaseUrlForSigning);

                    //send request and process response
                    var response = RestRequestWithPollyHelper.ExecuteRestRequestWithWaitAndRetry(client, request);
                    if (response.ResponseStatus != ResponseStatus.Completed || response.Content == null)
                    {
                        throw new HttpRequestException();
                    }

                    var invoiceDetailsResponse = JsonConvert.DeserializeObject<InvoiceDetailsByOrderLineItemResponse>(response.Content);
                    if (invoiceDetailsResponse == null) 
                        continue;

                    //add data to returned set
                    foreach (var invoiceDetailsOfOrderLineItem in invoiceDetailsResponse.InvoiceDetailsByOrderLineItems)
                    {
                        var updatedTransaction = transactionsWithoutInvoiceInformation.FirstOrDefault(t =>
                            t.OrderId == invoiceDetailsOfOrderLineItem.OrderLineItem.OrderId &&
                            t.OrderLineItemId == invoiceDetailsOfOrderLineItem.OrderLineItem.OrderLineItemId &&
                            t.ShipmentId == invoiceDetailsOfOrderLineItem.OrderLineItem.ShipmentId);
                        if (updatedTransaction == null)
                            continue;

                        foreach (var invoiceDetail in invoiceDetailsOfOrderLineItem.InvoiceDetails)
                        {
                            //if more than one invoice, create new transaction with each different invoice
                            updatedTransaction.InvoiceNumber = invoiceDetail.InvoiceNumber;
                            updatedTransaction.InvoiceIssueDate = invoiceDetail.InvoiceDate.GetValueOrDefault() != DateTime.MinValue ? invoiceDetail.InvoiceDate : null;
                            updatedTransaction.InvoiceDueDate = invoiceDetail.InvoiceDate.GetValueOrDefault() != DateTime.MinValue ? invoiceDetail.InvoiceDate.GetValueOrDefault().AddDays(45) : (DateTime?) null;
                            
                            //only want to return ones with an invoice number
                            if (updatedTransaction.InvoiceNumber != null) 
                                transactionsWithInvoiceInfo.Add(updatedTransaction);
                        }
                    }

                    //reset batch variables
                    batchCounter = 0;
                    batchedTransactions = new List<AmazonBusinessTransaction>();
                    batchedOrderLineItems = new List<OrderLineItem>();
                }
            }
            catch (Exception)
            {
                throw new HttpRequestException();
            }

            return transactionsWithInvoiceInfo;
        }
        
        /// <summary>
        /// Fetches shipping information for transactions from Amazon Reporting API.
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="lwaToken"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public List<AmazonBusinessTransaction> GetShippingInformationForAmazonBusinessTransactions(List<AmazonBusinessTransaction> transactions, string lwaToken)
        {
            var transactionsWithShippingInfo = new List<AmazonBusinessTransaction>();
            using var client = new RestClient(_baseUrl);
            try
            {
                foreach (var txn in transactions)
                {
                    var request = new RestRequest(_abOrdersGetOrderByOrderIdEndpoint + $"{txn.OrderId}");
                    request.AddHeader("x-amz-access-token", lwaToken);
                    request.AddQueryParameter("includeLineItems", true);
                    request.AddQueryParameter("includeCharges", true);
                    request.AddQueryParameter("includeShipments", true);
                    request = _signatureSigner.Sign(request, TrimmedBaseUrlForSigning);

                    var response = client.Execute(request);
                    if (response.ResponseStatus != ResponseStatus.Completed || response.Content == null)
                    {
                        transactionsWithShippingInfo.Add(txn);
                        continue;
                    }
                    
                    var ordersOutput = JsonConvert.DeserializeObject<OrdersOutput>(response.Content);
                    var order = ordersOutput?.Orders.FirstOrDefault();
                    if (order == null) continue;

                    //add returned info to new transaction
                    //txn.ShipmentTrackingNumber = string.Join(", ", order.Shipments.Select(s => s.CarrierTracking).Distinct() ?? new List<string>());
                    //txn.OrderStatus = order.Shipments.All(s => s.DeliveryInformation.DeliveryStatus == DeliveryStatus.Delivered) ? "Delivered" : "Shipped";
                    
                    /*
                    //update price from orders API if order API has a different amount than reconciliation
                    var matchedOrderLineItem =
                        (order.LineItems ?? Array.Empty<LineItem>()).FirstOrDefault(li =>
                            li.Title == txn.PurchaseTitle && li.UNSPSC == txn.UNSPSC);

                    if (txn.Quantity != matchedOrderLineItem?.ItemQuantity)
                    {
                        txn.Quantity = Convert.ToInt32(matchedOrderLineItem?.ItemQuantity);
                    }
                    if (txn.PaymentAmount != Convert.ToDecimal(matchedOrderLineItem?.ItemNetTotal.Amount))
                        {
                        txn.PaymentAmount = Convert.ToDecimal(matchedOrderLineItem?.ItemNetTotal.Amount);
                        }
                    */

                    //push txn to return list
                    transactionsWithShippingInfo.Add(txn);
                }
            }
            catch (Exception)
            {
                throw new HttpRequestException();
            }
            return transactionsWithShippingInfo;
        }
    }
}