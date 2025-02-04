namespace AmazonBusinessSalesPartnerAPIHelper
{
    public class TransactionsResponse
    {
        public List<Transaction> Transactions { get; set; }
        public string NextPageToken { get; set; }
    }
}