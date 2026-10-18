using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string MovementType { get; set; } 
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public int? PurchaseId { get; set; }
        public int? SaleId { get; set; }
        public string? Reason { get; set; }
        public StockAlert? Alert { get; set; }
    }
}