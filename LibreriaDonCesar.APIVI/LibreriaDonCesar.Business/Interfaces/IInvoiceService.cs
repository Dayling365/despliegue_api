using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IInvoiceService

    {
        Task<ServiceResponse<List<Invoice>>> InvoiceQueue();
        Task<ServiceResponse<Invoice>> ToPrint();

    }
}
