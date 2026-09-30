using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "El nombre de la marca es requerido")]
        [MaxLength(100, ErrorMessage = " El nombre no puede exceder 100 caracteres")]
        public string BrandName { get; set; }
    }
}
