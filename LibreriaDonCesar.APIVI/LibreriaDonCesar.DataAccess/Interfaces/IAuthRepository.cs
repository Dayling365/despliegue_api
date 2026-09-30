using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IAuthRepository
    {

        // 
        Task<RepositoryResponse<User>> RegisterAsync(User user);

        Task<RepositoryResponse<User>> GetByNameAsync(string name);

        Task<RepositoryResponse<User>> GetByEmailAsync(string email);

        Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId); 

    }
}
