
using System;
using System.Collections.Generic;

namespace HumanitarianProjectManagement.Models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int POID { get; set; }
        public int SupplierID { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmountDue { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public Supplier Supplier { get; set; }
    }
}
