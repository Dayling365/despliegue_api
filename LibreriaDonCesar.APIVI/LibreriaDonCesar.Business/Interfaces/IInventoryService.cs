using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IInventoryService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Inventory>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Inventory>> GetByProductIdAsync(int productId);
        Task<ServiceResponse<IEnumerable<Inventory>>> GetByNameAsync(string name);
        Task<ServiceResponse<Inventory>> UpdateMinStockAsync(int productId, int minStock);
        Task<ServiceResponse<PagedResponse<IEnumerable<Inventory>>>> GetLowStockAsync(PaginationParams pagination);
    }
}
