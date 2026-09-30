using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IUnitMeasureRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<UnitMeasure>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<UnitMeasure>> GetByIdAsync(int id);
        Task<RepositoryResponse<UnitMeasure>> GetByNameAsync(string name);
        Task<RepositoryResponse<UnitMeasure>> AddAsync(UnitMeasure unitMeasure);
        Task<RepositoryResponse<UnitMeasure>> UpdateAsync(int id, UnitMeasure unitMeasure);
        Task<RepositoryResponse<UnitMeasure>> SetStateAsync(int unitMeasureId, bool state);

    }
    
}
