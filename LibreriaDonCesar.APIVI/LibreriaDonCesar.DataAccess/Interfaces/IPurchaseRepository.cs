using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<PurchaseTransaction>> GetByIdAsync(int id);
        Task<RepositoryResponse<IEnumerable<PurchaseDetail>>> GetDetailByIdAsync(int purchaseId);
        Task<RepositoryResponse<PurchaseTransaction>> InsertAsync(Purchase master, IEnumerable<PurchaseDetail> details);
        Task<RepositoryResponse<List<PurchaseTransaction>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
