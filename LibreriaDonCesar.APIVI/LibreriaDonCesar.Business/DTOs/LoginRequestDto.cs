using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
   public  class LoginRequestDto
    {
        [Required,MinLength(5)]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }


    }
}
