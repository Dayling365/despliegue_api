using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class SaleTransaction
    {

        public Sale Master { get; set; }

        public IEnumerable<SaleDetail> Details { get; set; }
    }
}
