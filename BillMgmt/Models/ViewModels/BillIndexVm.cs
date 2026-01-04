using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.ViewModels
{
    public class BillIndexVm
    {
        public int BillId { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string StoreName { get; set; }
        public decimal TotalAmount { get; set; }

        // إجمالي الكمية داخل الفاتورة
        public decimal TotalQty { get; set; }
    }
}