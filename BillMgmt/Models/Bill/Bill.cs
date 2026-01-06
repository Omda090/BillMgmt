using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.Bill
{
    public class Bill
    {
        public int BillId { get; set; }
        public int CustomerId { get; set; }
        public int StoreId { get; set; }
        public DateTime BillDate { get; set; }
        public decimal TotalAmount { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    }
}