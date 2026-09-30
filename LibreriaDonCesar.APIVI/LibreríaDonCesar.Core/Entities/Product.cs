using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class Product
    {
        public  int Id { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public  string CategoryName { get; set; }
        public int PresentationId { get; set; }
        public string PresentationName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public decimal UnitPrice { get; set; }

        public string? Description { get; set; }
        public bool State {  get; set; }
    }
}
