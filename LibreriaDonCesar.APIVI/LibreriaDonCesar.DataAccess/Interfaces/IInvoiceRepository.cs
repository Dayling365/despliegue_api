using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<RepositoryResponse<ConcurrentQueue<Invoice>>> GetInvoiceQueue();
        Task<RepositoryResponse<Invoice>> ToPrint();

    }
}
