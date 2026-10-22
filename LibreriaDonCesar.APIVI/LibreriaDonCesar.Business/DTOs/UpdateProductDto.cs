using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdateProductDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la plantilla (BaseProductId) no es valido.")]
        public int BaseProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Id del color no es valido.")]
        public int? ColorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Id de la presentacion no es valido.")]
        public int? PresentationId { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo.")]
        public decimal SalePrice { get; set; }

        public bool State { get; set; }
    }
}
