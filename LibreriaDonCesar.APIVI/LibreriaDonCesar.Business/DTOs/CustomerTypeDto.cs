using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LibreriaDonCesar.Business.DTOs
{
    public class CustomerTypeDto
    {
        public int Id { get; set; }
        public string CustomerTypeName { get; set; } = string.Empty;
    }
}
