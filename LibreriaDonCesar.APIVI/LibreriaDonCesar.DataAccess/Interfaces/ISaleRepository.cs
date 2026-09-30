using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface ISaleRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id);
        Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details);
        Task<RepositoryResponse<List<SaleTransaction>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<RepositoryResponse<IEnumerable<SaleDetail>>> GetDetailByIdAsync(int saleId);

    }
}
