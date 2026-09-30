using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
   public class UpdateUnitMeasureDto
    {

        [Required]
        [StringLength(40, ErrorMessage = "El nombre de la unidad de medida no debe exceder los 40 caracteres.")]
        public string UnitMeasureName { get; set; }
        public bool State { get; set; }
    }
}
