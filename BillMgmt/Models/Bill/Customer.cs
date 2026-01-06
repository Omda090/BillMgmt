using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.Bill
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; }
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [StringLength(80)]
        public string City { get; set; }
    }
}