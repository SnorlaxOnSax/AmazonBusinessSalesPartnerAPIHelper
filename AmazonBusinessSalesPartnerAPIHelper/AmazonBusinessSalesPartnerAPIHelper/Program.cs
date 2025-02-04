using Microsoft.Extensions.Configuration;

namespace AmazonBusinessSalesPartnerAPIHelper;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Creating new AmazonBusinessClient with configuration from appSettings.json...");
        var client = new AmazonBusinessClient(new ConfigurationManager());
        var listOfTransactions = client.GetAmazonBusinessTransactions(client.GetLWAToken(), DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        
        Console.WriteLine(listOfTransactions);
    }
} 