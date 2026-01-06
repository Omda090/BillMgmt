using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BillMgmt.Models.ViewModels.Customer
{
    public class CustomerCreateVm
    {
        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [StringLength(150, ErrorMessage = "الاسم طويل")]
        public string CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [StringLength(30, ErrorMessage = "رقم الهاتف طويل")]
        public string PhoneNumber { get; set; }

        [StringLength(80, ErrorMessage = "اسم المدينة طويل")]
        public string City { get; set; }
    }
}