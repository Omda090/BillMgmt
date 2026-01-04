using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.Bill
{
    public class BillItem
    {
        public int BillItemId { get; set; }
        public int BillId { get; set; }

        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal Qty { get; set; }

        public virtual Bill Bill { get; set; }
        public virtual Product Product { get; set; }
    }
}