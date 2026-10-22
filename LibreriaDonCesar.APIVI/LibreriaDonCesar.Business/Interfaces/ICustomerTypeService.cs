using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface ICustomerTypeService
    {
        Task<ServiceResponse<IEnumerable<CustomerType>>> GetAllAsync();
    }
}
