using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.ViewModels
{
    public class BillItemVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }  // للعرض في Edit
        public decimal Price { get; set; }
        public decimal Qty { get; set; }
        public decimal LineTotal => Price * Qty;
    }
}