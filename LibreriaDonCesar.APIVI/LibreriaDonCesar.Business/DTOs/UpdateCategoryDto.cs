using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "El nombre de la categoria  debe tener entre 5 y 50 caracteres.")]
        public string CategoryName { get; set; }

        [StringLength(200, ErrorMessage = "La descripcion no debe tener mas de 200 caracteres.")]
        public string Description { get; set; }
        [Required]
        public bool State { get; set; }

    }
}
