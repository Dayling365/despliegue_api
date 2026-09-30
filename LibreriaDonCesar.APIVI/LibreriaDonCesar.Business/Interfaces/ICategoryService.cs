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
    public interface ICategoryService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Category>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Category>> GetByIdAsync(int id);
        Task <ServiceResponse<Category>> GetByNameAsync(string name);
        Task<ServiceResponse<Category>> CreateAsync(CreateCategoryDto newCategory);
        Task<ServiceResponse<Category>> UpdateAsync(int id, UpdateCategoryDto category);
        Task<ServiceResponse<Category>> SetStateAsync(int categoryId, bool state);
    }
}
