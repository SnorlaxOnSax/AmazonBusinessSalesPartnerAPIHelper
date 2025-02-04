using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonBusinessSalesPartnerAPIHelper
{
    public class AmazonBusinessTransaction
    {
        public int Id { get; set; }
        public bool Flagged { get; set; }
        public string FlaggedReason { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? InvoiceIssueDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string OrderId { get; set; }
        public string OrderLineItemId { get; set; }
        public string ShipmentId { get; set; }
        public string OrderStatus { get; set; }
        public string BusinessOrderInfo { get; set; }
        public string AmazonGroupName { get; set; }
        public string PurchaseTitle { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PurchasedBy { get; set; }
        public int Quantity { get; set; }
        public string AssignedTo { get; set; }
        public string UNSPSC { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        

        //coding values
        public string Fund { get; set; }
        public string Function { get; set; }
        public string Object { get; set; }	
        public string SubObject { get; set; }
        public string Organization { get; set; }
        public string FiscalYear { get; set; }
        public string ProgramIntentCode { get; set; }
        public string Region { get; set; }
        public string Division { get; set; }
        public string SubFund { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string CodingString { get; set; }
        
        //non-UI properties
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? OverriddenDate { get; set; }
        public string UpdatedBy { get; set; }
        public string OverriddenBy { get; set; }

        // Intacct Tracking
        public string IntacctStatus { get; set; }
        public DateTime? IntacctSendDate { get; set; }
        public int? IntacctBillRecordNumber { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not AmazonBusinessTransaction transaction)
            {
                return false;
            }
            
            return Flagged == transaction.Flagged
                   && TransactionDate == transaction.TransactionDate
                   && InvoiceIssueDate == transaction.InvoiceIssueDate
                   && InvoiceDueDate == transaction.InvoiceDueDate
                   && InvoiceNumber == transaction.InvoiceNumber
                   && OrderId == transaction.OrderId
                   && OrderLineItemId == transaction.OrderLineItemId
                   && ShipmentId == transaction.ShipmentId
                   && PurchaseTitle == transaction.PurchaseTitle
                   && PaymentAmount == transaction.PaymentAmount
                   && Quantity == transaction.Quantity
                   && CodingString == transaction.CodingString;
        }
    }
}