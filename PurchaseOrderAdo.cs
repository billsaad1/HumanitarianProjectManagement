using System;
using System.Collections.Generic;

namespace HumanitarianProjectManagement.Models
{
    public class PurchaseOrderAdo
    {
        public int POID { get; set; }
        public int PurchaseOrderID { get; set; }
        public int ProjectID { get; set; }
        public int? PRID { get; set; }
        public int? PurchaseID { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }
        public int? CreatedByUserID { get; set; }
        public int IssuedByUserID { get; set; }
        public int? ApprovedByUserID { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string PaymentTerms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<PurchaseOrderItem> Items { get; set; }

        public PurchaseOrderAdo()
        {
            Items = new HashSet<PurchaseOrderItem>();
        }
    }
}
