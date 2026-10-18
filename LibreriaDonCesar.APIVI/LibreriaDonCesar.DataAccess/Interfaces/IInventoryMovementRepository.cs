using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IInventoryMovementRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<InventoryMovement>> GetByIdAsync(int id);
        Task<RepositoryResponse<InventoryMovement>> AddAsync(InventoryMovement movement);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByProductIdAsync(int productId, PaginationParams pagination);
        Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetByTypeAsync(string movementType, PaginationParams pagination);
    }
}
