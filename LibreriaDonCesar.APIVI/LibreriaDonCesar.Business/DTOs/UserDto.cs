using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        // public string State {get;set;} 

    
        public List<string> Roles { get; set; } = new List<string>();

    }
}
