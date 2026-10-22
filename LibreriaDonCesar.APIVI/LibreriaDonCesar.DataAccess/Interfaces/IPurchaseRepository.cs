using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IPurchaseRepository
    {
        // Los listados devuelven solo la cabecera (Details vacío); el detalle se pide por Id
        Task<RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<PurchaseTransaction>> GetByIdAsync(int id);
        Task<RepositoryResponse<IEnumerable<PurchaseDetail>>> GetDetailByIdAsync(int purchaseId);
        Task<RepositoryResponse<PurchaseTransaction>> InsertAsync(Purchase master, IEnumerable<PurchaseDetail> details);
        Task<RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
    }
}
