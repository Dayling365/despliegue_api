using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface ICategoryRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Category>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Category>> GetByIdAsync(int id);
        Task<RepositoryResponse<Category>> GetByNameAsync(string name);
        Task<RepositoryResponse<Category>> AddAsync(Category category);
        Task<RepositoryResponse<Category>> UpdateAsync(int id, Category category);
        Task<RepositoryResponse<Category>> SetStateAsync(int categoryId, bool state);

    }
}
