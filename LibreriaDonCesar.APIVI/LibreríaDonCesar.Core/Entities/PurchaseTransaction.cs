using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Core.Entities
{
    public class PurchaseTransaction
    {
        public Purchase Master { get; set; }
        public IEnumerable<PurchaseDetail> Details { get; set; }
    }
}
