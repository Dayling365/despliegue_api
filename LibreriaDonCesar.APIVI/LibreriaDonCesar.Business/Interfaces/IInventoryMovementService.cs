using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IInventoryMovementService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<InventoryMovement>> GetByIdAsync(int id);
        Task<ServiceResponse<InventoryMovement>> CreateAsync(CreateInventoryMovementDto newMovement);
        Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
        Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByProductIdAsync(int productId, PaginationParams pagination);
        Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByTypeAsync(string movementType, PaginationParams pagination);
    }
}
