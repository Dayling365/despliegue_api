using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.DTOs
{
    public class SaleResponseDto
    {
        public int Id { get; set; }

        public string CustomerType { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<SaleResponseDetailDto> Details { get; set; }




    }
}
