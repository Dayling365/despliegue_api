using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class Presentation
    {
        public int Id { get; set; }
        public string? PresentationName { get; set; }
        public decimal Amount { get; set; }
        public int UnitMeasureId { get; set; }
       
        public bool State {  get; set; }

        public string UnitMeasureName { get; set; }
    }
}
