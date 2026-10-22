using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateBaseProductDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "El nombre del producto no debe exceder los 50 caracteres.")]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Id de la categoria no es valido.")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Id de la marca no es valido.")]
        public int BrandId { get; set; }
    }
}
