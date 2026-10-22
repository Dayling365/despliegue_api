using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface ISaleRepository
    {
        // Los listados devuelven solo la cabecera (Details vacío); el detalle se pide por Id
        Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id);
        // Master.CustomerTypeId + líneas (ProductId, Quantity). Devuelve venta, detalle y alertas de stock.
        Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details);
        Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
        Task<RepositoryResponse<IEnumerable<SaleDetail>>> GetDetailByIdAsync(int saleId);
    }
}
