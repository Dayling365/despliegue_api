using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<PagedResponse<IEnumerable<Product>>>> GetAllAsync(PaginationParams pagination);
        Task<ServiceResponse<Product>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<Product>>> GetByNameAsync(string name);
        Task<ServiceResponse<Product>> CreateAsync(CreateProductDto newProduct);
        Task<ServiceResponse<Product>> UpdateAsync(int id, UpdateProductDto product);
        Task<ServiceResponse<Product>> SetStateAsync(int productId, bool state);
    }
}
