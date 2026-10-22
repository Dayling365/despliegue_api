using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using LibreriaDonCesar.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly IInventoryMovementRepository _inventoryMovementRepository;

        public InventoryMovementService(IInventoryMovementRepository inventoryMovementRepository)
        {
            _inventoryMovementRepository = inventoryMovementRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _inventoryMovementRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50037:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<InventoryMovement>> GetByIdAsync(int id)
        {
            var repoResponse = await _inventoryMovementRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    return new ServiceResponse<InventoryMovement>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"
                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 0:
                    case 50050:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryMovement>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };
            }
        }

        // Entrada / Salida / Baja: Quantity = unidades que se mueven. Ajuste: Quantity = nuevo stock final.
        public async Task<ServiceResponse<InventoryMovement>> CreateAsync(CreateInventoryMovementDto newMovement)
        {
            try
            {
                if (newMovement.ProductId == 0)
                {
                    return new ServiceResponse<InventoryMovement>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id del producto no es valido"
                    };
                }

                var movement = new InventoryMovement()
                {
                    ProductId = newMovement.ProductId,
                    MovementType = newMovement.MovementType,
                    Quantity = newMovement.Quantity,
                    Reason = newMovement.Reason,
                };

                var result = await _inventoryMovementRepository.AddAsync(movement);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<InventoryMovement>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Movimiento registrado con exito",
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50050:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No existe inventario para el producto indicado"
                        };

                    case 50060:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Stock insuficiente para completar la operación"
                        };

                    case 50061:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "El tipo de movimiento no es válido (Entrada, Salida, Ajuste o Baja)"
                        };

                    case 50062:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "La cantidad indicada no es válida"
                        };

                    default:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Ocurrio un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryMovement>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination)
        {
            var result = await _inventoryMovementRepository.GetByDateRangeAsync(startDate, endDate, pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50037:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay movimientos en el rango indicado"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByProductIdAsync(int productId, PaginationParams pagination)
        {
            var result = await _inventoryMovementRepository.GetByProductIdAsync(productId, pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50037:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "El producto no tiene movimientos"
                    };

                case 50050:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un producto asociado al Id proporcionado"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetMovementsByTypeAsync(string movementType, PaginationParams pagination)
        {
            var result = await _inventoryMovementRepository.GetByTypeAsync(movementType, pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50037:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay movimientos de ese tipo"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<InventoryMovement>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }
    }
}
