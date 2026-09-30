using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreatePurchaseDetailDto
    {
        [Required(ErrorMessage = "El Id del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El Id del producto no es valido.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad del producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad del producto ingresado no es valido")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El precio del producto es obligatorio")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El precio del producto no es valido.")]
        public int PurchasePrice { get; set; }
       
    }
}
