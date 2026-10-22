using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class PurchaseResponseDetailDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ColorName { get; set; }
        public string? PresentationName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
