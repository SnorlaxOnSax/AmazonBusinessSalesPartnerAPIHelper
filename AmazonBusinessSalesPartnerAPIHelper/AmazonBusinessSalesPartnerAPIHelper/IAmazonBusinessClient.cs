using AmazonBusinessSalesPartnerAPIHelper.ResponseObjects;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    public interface IAmazonBusinessClient
    {
        string GetLWAToken();
        List<AmazonBusinessTransaction> GetAmazonBusinessTransactions(string lwaToken, DateTime startDate, DateTime endDate);
        List<AmazonBusinessTransaction> GetInvoiceInformationForAmazonBusinessTransactions(List<AmazonBusinessTransaction> transactions, string lwaToken);
        List<AmazonBusinessTransaction> GetShippingInformationForAmazonBusinessTransactions(List<AmazonBusinessTransaction> transactions, string lwaToken);
        List<AmazonBusinessTransaction> CheckForUpdatedInvoiceInfo(string lwaToken, List<AmazonBusinessTransaction> transactionsWithoutInvoiceInformation);
    }
}