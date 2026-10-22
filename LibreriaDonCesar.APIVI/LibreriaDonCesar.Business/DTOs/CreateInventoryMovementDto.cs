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
        [Range(1, int.MaxValue, ErrorMessage = "El producto es requerido")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public string MovementType { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Quantity { get; set; }

        [StringLength(200, ErrorMessage = "El motivo no debe exceder los 200 caracteres.")]
        public string? Reason { get; set; }
    }
}
