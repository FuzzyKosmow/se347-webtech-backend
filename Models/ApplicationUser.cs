using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ecommerce_api.Models
{
    //Used for both admin and user
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;


        // Navigation Properties
        // Voucher used by the user
        public List<Voucher> Vouchers { get; set; }



    }
}