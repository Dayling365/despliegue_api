using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateSaleDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de cliente (CustomerTypeId) es requerido")]
        public int CustomerTypeId { get; set; }

        [Required(ErrorMessage = " La venta debe incluir un detalle de compra")]
        [MinLength(1, ErrorMessage = "El detalle debe incluir el menos un producto")]
        public List<CreateSaleDetailDto> Details { get; set; } = new List<CreateSaleDetailDto>();
    }
}
