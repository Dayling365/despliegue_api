using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IInventoryRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Inventory>> GetByProductIdAsync(int productId);
        // Todas las variantes de la plantilla con ese nombre
        Task<RepositoryResponse<IEnumerable<Inventory>>> GetByNameAsync(string name);
        Task<RepositoryResponse<Inventory>> UpdateMinStockAsync(int productId, int minStock);
        // Productos en el umbral mínimo o por debajo
        Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetLowStockAsync(PaginationParams pagination);
    }
}
