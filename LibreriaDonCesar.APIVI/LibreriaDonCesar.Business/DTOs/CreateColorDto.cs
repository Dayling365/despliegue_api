using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateColorDto
    {
        [Required(ErrorMessage = "El nombre del color es requerido")]
        [MaxLength(50, ErrorMessage ="El nombre no puede exceder los 50 caracteres")]
        public string ColorName { get; set; }
    }
}
