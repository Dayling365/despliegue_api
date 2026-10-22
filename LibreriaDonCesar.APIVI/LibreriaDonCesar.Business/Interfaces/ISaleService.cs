using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface ISaleService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllSaleAsync(PaginationParams pagination);
        Task<ServiceResponse<SaleTransaction>> GetByIdAsync(int id);
        Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto);
        Task<ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
        Task<ServiceResponse<IEnumerable<SaleResponseDetailDto>>> GetDetailByIdAsync(int saleId);
    }
}
