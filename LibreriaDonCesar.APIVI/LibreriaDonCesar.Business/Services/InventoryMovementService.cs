using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
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

        public async Task<ServiceResponse<InventoryMovement>> CreateAsync(CreateInventoryMovementDto newMovement)
        {
            try
            {
                // mapeo
                var movement = new InventoryMovement()
                {
                    ProductId = newMovement.ProductId,
                    MovementType = newMovement.MovementType,
                    Quantity = newMovement.Quantity,
                    Reason = newMovement.Reason
                };

                // llamado al metodo de repo
                var result = await _inventoryMovementRepository.AddAsync(movement);

                return new ServiceResponse<InventoryMovement>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryMovement>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<InventoryMovement>> GetByIdAsync(int id)
        {
            var repositoryResponse = await _inventoryMovementRepository.GetByIdAsync(id);

            try
            {

                if (repositoryResponse.OperationStatusCode ==0)
                {
                    return new ServiceResponse<InventoryMovement>
                    {
                        Data = repositoryResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repositoryResponse.Message ?? "Operacion exitosa"
                    };
                }
                switch (repositoryResponse.OperationStatusCode)
                {
                    case 50100:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            //notfound
                            MessageCode = MessageCodes.NotFound,
                            Message = repositoryResponse.Message ?? "No existe el movimiento de inventario"
                        };

                    default:
                        return new ServiceResponse<InventoryMovement>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repositoryResponse.Message ?? "Ocurrio un error inesperado"
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
                    Message = repositoryResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<List<InventoryMovement>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var repoResponse = await _inventoryMovementRepository.GetMovementsByDateRangeAsync(startDate, endDate);

                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<List<InventoryMovement>>
                    {
                        Data = repoResponse.Data.ToList(),
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Movimeinto de inventario obtenidos exitosamente"
                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 50037:
                        return new ServiceResponse<List<InventoryMovement>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No se encontraron movimiento de inventario en el rango proporcionado"
                        };

                    default:
                        return new ServiceResponse<List<InventoryMovement>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                        };
                }

               
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<InventoryMovement>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<InventoryMovement>>> GetMovementsByProductIdAsync(int productId)
        {
            var repositoryResponse = await _inventoryMovementRepository.GetMovementsByProductIdAsync(productId);

            try
            {
                if (repositoryResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<InventoryMovement>>
                    {
                        Data = repositoryResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repositoryResponse.Message ?? "Operacion exitosa"
                    };
                }
                switch (repositoryResponse.OperationStatusCode)
                {
                    case 50100:
                        return new ServiceResponse<IEnumerable<InventoryMovement>>
                        {
                            Data = null,
                            IsSuccess = false,
                            //notfound
                            MessageCode = MessageCodes.NotFound,
                            Message = repositoryResponse.Message ?? "No existe el movimiento de inventario de ese producto"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<InventoryMovement>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repositoryResponse.Message ?? "Ocurrio un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repositoryResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<IEnumerable<InventoryMovement>>> GetMovementsByTypeAsync(string movementType)
        {
            var repoResponse = await _inventoryMovementRepository.GetMovementsByTypeAsync(movementType);
            try
            {
               

                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<InventoryMovement>>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"
                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 50037:
                        return new ServiceResponse<IEnumerable<InventoryMovement>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontraron movimientos para el tipo proporcionado"
                        };

                   default:
                        return new ServiceResponse<IEnumerable<InventoryMovement>>
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
                return new ServiceResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }



    }





}

