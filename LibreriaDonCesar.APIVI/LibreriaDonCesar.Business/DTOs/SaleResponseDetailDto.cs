using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class SaleResponseDetailDto
    {
       // public int Id { get; set; }

        //public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal SalePrice { get; set; }

        public decimal LineTotal { get; set; }


    }
}
