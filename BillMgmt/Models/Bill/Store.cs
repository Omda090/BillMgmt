using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.Bill
{
    public class Store
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
    }
}