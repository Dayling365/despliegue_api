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
    public interface ISupplierService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Supplier>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Supplier>> GetByIdAsync(int id);
        Task<ServiceResponse<Supplier>> GetByNameAsync(string name);
        Task<ServiceResponse<Supplier>> CreateAsync(CreateSupplierDto newSupplier);

        Task<ServiceResponse<Supplier>> UpdateAsync(int id, UpdateSupplierDto supplier);
        Task<ServiceResponse<Supplier>> SetStateAsync(int supplierId, bool state);


    }
}
