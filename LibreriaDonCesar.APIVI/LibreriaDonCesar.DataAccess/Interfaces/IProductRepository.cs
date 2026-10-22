using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.DataAccess.Interfaces
{
    public interface IProductRepository
    {
        Task<RepositoryResponse<PagedResponse<IEnumerable<Product>>>> GetAllAsync(PaginationParams pagination);
        Task<RepositoryResponse<Product>> GetByIdAsync(int id);
        Task<RepositoryResponse<IEnumerable<Product>>> GetByNameAsync(string name);
        Task<RepositoryResponse<Product>> AddAsync(Product product);
        Task<RepositoryResponse<Product>> UpdateAsync(int id, Product product);
        Task<RepositoryResponse<Product>> SetStateAsync(int productId, bool state);
    }
}
