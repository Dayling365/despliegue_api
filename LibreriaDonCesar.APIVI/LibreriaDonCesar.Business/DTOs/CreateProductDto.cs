using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(60, ErrorMessage = "El nombre del producto no debe exceder los 50 caracteres.")]
        public string? ProductName { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la categoria no es valido.")]
        public int CategoryId { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la presentacion no es valido.")]
        public int PresentationId { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la Marca no es valido.")]
        public int BrandId { get; set; }

        [DefaultValue(0)]
        [Range(1, int.MaxValue, ErrorMessage = "El Id del Color no es valido.")]
        public int ColorId { get; set; }

        public decimal UnitPrice { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no debe exceder los 100 caracteres.")]
        public string? Description { get; set; }
    }
}
