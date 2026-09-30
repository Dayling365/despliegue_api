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
    public interface IInventoryMovementService
    {

        Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination);

        Task<ServiceResponse<InventoryMovement>> CreateAsync(CreateInventoryMovementDto newMovement);
        Task<ServiceResponse<InventoryMovement>> GetByIdAsync(int id);
        Task<ServiceResponse<List<InventoryMovement>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResponse<IEnumerable<InventoryMovement>>> GetMovementsByProductIdAsync(int productId);
        Task<ServiceResponse<IEnumerable<InventoryMovement>>> GetMovementsByTypeAsync(string movementType);
    }
}
