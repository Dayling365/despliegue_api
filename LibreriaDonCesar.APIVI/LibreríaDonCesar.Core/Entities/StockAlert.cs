using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class StockAlert
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int UnitsInStock { get; set; }
        public int MinStock { get; set; }
        public string? StockStatus { get; set; }

    }
}
