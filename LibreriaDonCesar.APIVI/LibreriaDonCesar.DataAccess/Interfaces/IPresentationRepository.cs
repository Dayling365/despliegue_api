using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IPresentationRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Presentation>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Presentation>> GetByIdAsync(int id);
        Task<RepositoryResponse<Presentation>> GetByNameAsync(string name);
        Task<RepositoryResponse<Presentation>> AddAsync(Presentation presentation);
        Task<RepositoryResponse<Presentation>> UpdateAsync(int id, Presentation presentation);
        Task<RepositoryResponse<Presentation>> SetStateAsync(int presentationId, bool state);

    }
}
