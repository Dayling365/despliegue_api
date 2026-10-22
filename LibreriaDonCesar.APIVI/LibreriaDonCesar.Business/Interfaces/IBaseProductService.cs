using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IBaseProductService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<BaseProduct>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<BaseProduct>> GetByIdAsync(int id);
        Task<ServiceResponse<BaseProduct>> CreateAsync(CreateBaseProductDto dto);
        Task<ServiceResponse<BaseProduct>> UpdateAsync(int id, UpdateBaseProductDto dto);
        Task<ServiceResponse<BaseProduct>> SetStateAsync(int id, bool state);
    }
}
