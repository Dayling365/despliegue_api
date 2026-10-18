using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IBaseProductRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<BaseProduct>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<BaseProduct>> GetByIdAsync(int id);
        Task<RepositoryResponse<BaseProduct>> AddAsync(BaseProduct baseProduct);
        Task<RepositoryResponse<BaseProduct>> UpdateAsync(int id, BaseProduct baseProduct);
        Task<RepositoryResponse<BaseProduct>> SetStateAsync(int id, bool state);
    }
}
