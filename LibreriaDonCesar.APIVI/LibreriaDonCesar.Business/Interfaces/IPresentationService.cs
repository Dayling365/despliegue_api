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
    public interface IPresentationService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Presentation>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Presentation>> GetByIdAsync(int id);
        Task<ServiceResponse<Presentation>> GetByNameAsync(string name);
        Task<ServiceResponse<Presentation>> CreateAsync(CreatePresentationDto newPresentation);
        Task<ServiceResponse<Presentation>> UpdateAsync(int id, UpdatePresentationDto presentation);
        Task<ServiceResponse<Presentation>> SetStateAsync(int presentationId, bool state);
    }
}
