using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<User>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<User>> GetByIdAsync(int ids);
        Task<ServiceResponse<User>> GetByNameAsync(string name);
        Task<ServiceResponse<User>> CreateAsync(CreateUserDto newUser);
        Task<ServiceResponse<User>> UpdateAsync(int id, UpdateUserDto user);

        Task<ServiceResponse<UserRole>> AssignRoleAsync(int userId, int roleId);
        Task<ServiceResponse<User>> SetStateAsync(int userId, bool state);






    }
}
