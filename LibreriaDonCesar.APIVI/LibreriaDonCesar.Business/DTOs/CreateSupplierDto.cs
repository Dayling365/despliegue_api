using System.ComponentModel.DataAnnotations;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateSupplierDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no debe exceder los 50 caracteres.")]
        public string SupplierName { get; set; }
     
        [Required]
        [StringLength(50, ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; }

    }
}
