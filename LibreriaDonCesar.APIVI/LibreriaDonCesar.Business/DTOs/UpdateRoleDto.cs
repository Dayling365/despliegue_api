using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UpdateRoleDto
    {

        [Required]
        [StringLength(40, ErrorMessage = "El nombre del rol no debe exceder los 40 caracteres.")]
        public string RoleName { get; set; }
        public bool State { get; set; }
    }
}
