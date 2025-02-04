namespace AmazonBusinessSalesPartnerAPIHelper.ResponseObjects
{
    public class OrdersOutput
    {
        public IEnumerable<Order> Orders { get; set; }
        public string NextPageToken { get; set; }
        public int Size { get; set; }
    }
}