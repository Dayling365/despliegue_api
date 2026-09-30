using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.DTOs
{
    public class PresentationDto
    {
        public int Id { get; set; }
        public string? PresentationName { get; set; }
        public decimal Amount { get; set; }
        public int UnitMeasureId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string UnitMeasureName { get; set; }
  
        //public bool State { get; set; }
    }
}
