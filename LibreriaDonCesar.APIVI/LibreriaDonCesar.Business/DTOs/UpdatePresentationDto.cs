using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdatePresentationDto
    {

        [Required]
        [StringLength(60, ErrorMessage = "El nombre de la presentación no debe exceder los 60 caracteres.")]
        public string? PresentationName { get; set; }

        public decimal Amount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la unidad de medida no es valido.")]
        public int UnitMeasureId { get; set; }
    
        [Required]
        public bool State { get; set; }
    }
}
