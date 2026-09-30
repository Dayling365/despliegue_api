using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface ISaleService
    {

        Task<ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllSaleAsync(PaginationParams pagination);
        Task<ServiceResponse<SaleTransaction>> GetByIdAsync(int id);
        Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto);
        Task<ServiceResponse<List<SaleResponseDto>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResponse<IEnumerable<SaleResponseDetailDto>>> GetDetailByIdAsync(int saleId);


    }
}
