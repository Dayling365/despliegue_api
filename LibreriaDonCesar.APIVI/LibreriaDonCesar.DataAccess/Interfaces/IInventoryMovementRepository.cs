using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IInventoryMovementRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<InventoryMovement>> GetByIdAsync(int id);
        // Registra el movimiento; Alert viene lleno si el stock quedó en el umbral mínimo o por debajo
        Task<RepositoryResponse<InventoryMovement>> AddAsync(InventoryMovement movement);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByProductIdAsync(int productId, PaginationParams pagination);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByTypeAsync(string movementType, PaginationParams pagination);
    }
}
