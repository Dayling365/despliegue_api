using System.ComponentModel.DataAnnotations;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no debe exceder los 50 caracteres.")]
        public string CategoryName { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "La descripción debe tener entre 5 y 200 caracteres.")]
        public string Description { get; set; }
    }
}
