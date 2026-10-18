using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LibreriaDonCesar.Core.Entities
{
    public class BaseProduct
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public bool State { get; set; }
    }
}
