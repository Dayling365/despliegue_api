using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdateSupplierDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no debe exceder los 50 caracteres.")]
        public string SupplierName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; }
        public bool State { get; set; }
    }
}
