using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Services
{
    public interface IPurchaseService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllPurchasesAsync(PaginationParams pagination);
        Task<ServiceResponse<PurchaseTransaction>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>> GetDetailByIdAsync(int purchaseId);
        Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto);
        Task<ServiceResponse<List<PurchaseResponseDto>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}
