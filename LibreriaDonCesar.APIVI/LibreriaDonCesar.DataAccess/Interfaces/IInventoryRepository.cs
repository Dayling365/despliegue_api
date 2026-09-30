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
        Task<RepositoryResponse<Inventory>> GetByIdAsync(int id);
        Task<RepositoryResponse<Inventory>> GetByNameAsync(string name);


    }
}
