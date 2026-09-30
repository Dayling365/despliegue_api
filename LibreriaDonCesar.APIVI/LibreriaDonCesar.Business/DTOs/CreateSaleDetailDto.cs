using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateSaleDetailDto
    {

        [Required(ErrorMessage = " El id el producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del producto ingresado no es valido")]

        public int ProductId { get; set; }


        [Required(ErrorMessage = " La cantidad del producto es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = " La cantidad del producto ingresada no es valida")]

        public int Quantity { get; set; }


    }
}
