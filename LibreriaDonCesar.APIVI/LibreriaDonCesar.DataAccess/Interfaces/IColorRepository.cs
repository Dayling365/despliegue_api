using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = LibreriaDonCesar.Core.Entities.Color;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IColorRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Color>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Color>> GetByIdAsync(int id);
        Task<RepositoryResponse<Color>> GetByNameAsync(string name);
        Task<RepositoryResponse<Color>> AddAsync(Color color);
        Task<RepositoryResponse<Color>> UpdateAsync(int id, Color color);
        Task<RepositoryResponse<Color>> SetStateAsync(int id, bool state);
    }
}