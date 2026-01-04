using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.ViewModels
{
    public class BillListVm
    {
        public int BillId { get; set; }
        public DateTime BillDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}