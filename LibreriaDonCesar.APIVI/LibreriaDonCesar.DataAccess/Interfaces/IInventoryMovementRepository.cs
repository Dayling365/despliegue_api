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

        Task<RepositoryResponse<InventoryMovement>> AddAsync(InventoryMovement movement);
        Task<RepositoryResponse<InventoryMovement>> GetByIdAsync(int id);
        Task<RepositoryResponse<List<InventoryMovement>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<RepositoryResponse<IEnumerable<InventoryMovement>>> GetMovementsByProductIdAsync(int productId);
        Task<RepositoryResponse<IEnumerable<InventoryMovement>>> GetMovementsByTypeAsync(string movementType);

    }
}
