using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = LibreriaDonCesar.Core.Entities.Color;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IColorService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Color>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Color>> GetByIdAsync(int id);
        Task<ServiceResponse<Color>> GetByNameAsync(string name);
        Task<ServiceResponse<Color>> CreateAsync(CreateColorDto newColor);
        Task<ServiceResponse<Color>> UpdateAsync(int id, UpdateColorDto color);
        Task<ServiceResponse<Color>> SetStateAsync(int id, bool state);
    }
}