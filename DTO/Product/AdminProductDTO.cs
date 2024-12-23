using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce_api.DTO.Product
{
    public class AdminProductDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Discount_price { get; set; }
        public float Rating { get; set; }
        public bool Availability { get; set; }
        public decimal ImportPrice { get; set; }
        public string Country { get; set; }
        public string RelatedCity { get; set; }
        public List<string> Images { get; set; }
        public string Description { get; set; }

        public bool? IsBestSeller { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool? IsNewArrival { get; set; }
        public bool? IsPopular { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}