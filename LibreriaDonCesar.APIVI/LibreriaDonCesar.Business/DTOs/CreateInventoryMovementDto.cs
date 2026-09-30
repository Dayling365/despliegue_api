using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class CreateInventoryMovementDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public string MovementType { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Quantity { get; set; }

        public string Reason { get; set; }
    }
}