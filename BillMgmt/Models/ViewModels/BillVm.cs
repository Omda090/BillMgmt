using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BillMgmt.Models.ViewModels
{
    public class BillVm
    {

        public int? BillId { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار العميل")]
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار المخزن")]
        public int? StoreId { get; set; }

        public decimal TotalAmount { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> Stores { get; set; }

        public List<BillItemVm> Items { get; set; }
    }
}