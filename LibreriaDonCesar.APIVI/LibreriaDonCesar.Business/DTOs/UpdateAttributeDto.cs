using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdateAttributeDto
    {
        [Required(ErrorMessage = "El nombre del attributo es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string AttributeName { get; set; }

        [Required]
        public bool State { get; set; }
    }
}
