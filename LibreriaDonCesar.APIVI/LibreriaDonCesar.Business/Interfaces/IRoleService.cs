using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Role>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Role>> GetByIdAsync(int id);
        Task<ServiceResponse<Role>> GetByNameAsync(string name);
        Task<ServiceResponse<Role>> CreateAsync(CreateRoleDto newRole);

        Task<ServiceResponse<Role>> UpdateAsync(int id, UpdateRoleDto role);
        Task<ServiceResponse<Role>> SetStateAsync(int roleId, bool state);
    }
}
