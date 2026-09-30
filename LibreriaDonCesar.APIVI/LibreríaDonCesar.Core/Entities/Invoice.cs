using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class Invoice
    {
       public int Id { get; set; }
         public DateTime CreatedDate { get; set; }
        public int SaleId { get; set; }
        public bool IsPrinted { get; set; }
       public decimal TotalAmount { get; set; }

    }
}
