using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int BaseProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? PresentationId { get; set; }
        public string? PresentationName { get; set; }
        public decimal SalePrice { get; set; }
        public bool State { get; set; }
    }
}
