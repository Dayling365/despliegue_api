using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IInventoryRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Inventory>> GetByProductIdAsync(int productId);
        Task<RepositoryResponse<IEnumerable<Inventory>>> GetByNameAsync(string name);
        Task<RepositoryResponse<Inventory>> UpdateMinStockAsync(int productId, int minStock);
        Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetLowStockAsync(PaginationParams pagination);
    }
}
