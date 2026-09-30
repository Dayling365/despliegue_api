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
    public interface IInventoryService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Inventory>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Inventory>> GetByIdAsync(int id);
        Task<ServiceResponse<Inventory>> GetByNameAsync(string name);


    }
}
