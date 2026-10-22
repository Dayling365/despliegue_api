using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Services
{
    public interface IPurchaseService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllPurchasesAsync(PaginationParams pagination);
        Task<ServiceResponse<PurchaseTransaction>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>> GetDetailByIdAsync(int purchaseId);
        Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto);
        Task<ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
    }
}
