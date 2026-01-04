using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.Bill
{
    public class StoreProduct
    {
        public int StoreProductId { get; set; }
        public int StoreId { get; set; }
        public int ProductId { get; set; }
        public decimal StockQty { get; set; }
        public decimal Price { get; set; }

        public virtual Store Store { get; set; }
        public virtual Product Product { get; set; }
    }
}