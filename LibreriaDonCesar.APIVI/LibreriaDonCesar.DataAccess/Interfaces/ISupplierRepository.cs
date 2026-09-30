using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface ISupplierRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Supplier>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Supplier>> GetByIdAsync(int id);

        Task<RepositoryResponse<Supplier>> GetByNameAsync(string name);

        Task<RepositoryResponse<Supplier>> AddAsync(Supplier supplier);
        Task<RepositoryResponse<Supplier>> UpdateAsync(int id, Supplier supplier);
        Task<RepositoryResponse<Supplier>> SetStateAsync(int supplierId, bool state);
    }
}
