using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<User>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<User>> GetByIdAsync(int id);
        Task<RepositoryResponse<User>> GetByNameAsync(string name);
        Task<RepositoryResponse<User>> AddAsync(User usre);
        Task<RepositoryResponse<User>> UpdateAsync(int id, User usre);
        Task<RepositoryResponse<User>> SetStateAsync(int userId, bool state);

        Task<RepositoryResponse<UserRole>> AssignRoleAsync(int userId, int roleId);







    }
}
