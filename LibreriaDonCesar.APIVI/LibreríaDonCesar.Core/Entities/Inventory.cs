using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ColorName { get; set; }
        public string? PresentationName { get; set; }
        public decimal SalePrice { get; set; }
        public int UnitsInStock { get; set; }
        public int MinStock { get; set; }
        public bool IsLowStock { get; set; }
        public string? StockStatus { get; set; }
        public int UnitsMissing { get; set; }
    }
}

    

