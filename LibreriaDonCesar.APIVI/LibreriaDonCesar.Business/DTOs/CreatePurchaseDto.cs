using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreatePurchaseDto
    {
        [Required(ErrorMessage = "El Id del proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El Id del proveedor no es valido.")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "La compra debe incluir un detalle de compra")]
        [MinLength(1, ErrorMessage = "El detalle debe incluir al menos un producto")]
        public List<CreatePurchaseDetailDto> Details { get; set; } = new List<CreatePurchaseDetailDto>();
    }
}
