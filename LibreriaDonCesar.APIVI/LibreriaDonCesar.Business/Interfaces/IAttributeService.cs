using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = LibreriaDonCesar.Core.Entities.Attribute;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IAttributeService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Attribute>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Attribute>> GetByIdAsync(int id);
        Task<ServiceResponse<Attribute>> GetByNameAsync(string name);
        Task<ServiceResponse<Attribute>> CreateAsync(CreateAttributeDto newAttribute);
        Task<ServiceResponse<Attribute>> UpdateAsync(int id, UpdateAttributeDto attribute);
        Task<ServiceResponse<Attribute>> SetStateAsync(int attributeId, bool state);
    }
}
