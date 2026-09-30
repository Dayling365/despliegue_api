using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using LibreriaDonCesar.DataAccess.Repositories;

namespace LibreriaDonCesar.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IUserService _userService;
        public SaleService(ISaleRepository saleRepository, IInventoryService inventoryService, IUserService userService)
        {

            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _userService = userService;
        }



        public async Task<ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllSaleAsync(PaginationParams pagination)

        {
            var result = await _saleRepository.GetAllAsync(pagination);


            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<SaleTransaction>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }
        
        public async Task<ServiceResponse<SaleTransaction>> GetByIdAsync(int id)
        {
            try
            {
                var repoResponse = await _saleRepository.GetByIdAsync(id);
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<SaleTransaction>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta obtenida exitosamente."
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50071:
                        return new ServiceResponse<SaleTransaction>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };
                    default:
                        return new ServiceResponse<SaleTransaction>
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
                return new ServiceResponse<SaleTransaction>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto)
        {
            try
            {

             


                foreach (var detail in dto.Details)
                {
                    var existenProduct = await _inventoryService.GetByIdAsync(detail.ProductId);
                    if (existenProduct.Data! == null)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $" El producto con id {detail.ProductId} no existe en el inventario "


                        };
                    }
                    // valida si existen unidad hay suficientes unidades en inventaio para  venta
                    if (existenProduct.Data!.UnitsInStock < detail.Quantity)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = $"No existen suficientes unidades en el inventario para el producto con id {detail.ProductId} "

                        };
                    }
                }

                var saleMaster = new Sale
                {
                    CustomerType = dto.CustomerType,
                    SaleDate = DateTime.Now,
                    TotalAmount = 0
                };

                var saleDetail = dto.Details.Select(dt => new SaleDetail
                {
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity,
                }).ToList();



                var repoResponse = await _saleRepository.InsertAsync(saleMaster, saleDetail);

                if (repoResponse.OperationStatusCode == 0)
                {

                    var dataResponse = new SaleResponseDto();
                    dataResponse.Id = repoResponse.Data!.Master.Id;
                    dataResponse.CustomerType = repoResponse.Data!.Master.CustomerType;
                    dataResponse.SaleDate = repoResponse.Data!.Master.SaleDate;
                    dataResponse.TotalAmount = repoResponse.Data!.Master.TotalAmount;

                    dataResponse.Details = repoResponse.Data.Details.Select(dt => new SaleResponseDetailDto
                    {
                        ProductId = dt.ProductId,
                        ProductName=dt.ProductName,
                        Quantity = dt.Quantity,
                        SalePrice = dt.SalePrice,
                        LineTotal = dt.LineTotal
                    }).ToList();
                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta registrada  exitosamente "

                    };
                }
                else
                {
                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = " Hubo un error al registrada la Venta "

                    };
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = " Ocurrio un error inesperado "

                };
            }


        }

        public async Task<ServiceResponse<IEnumerable<SaleResponseDetailDto>>> GetDetailByIdAsync(int saleId)
        {
            try
            {
                var repoResponse = await _saleRepository.GetDetailByIdAsync(saleId);

                if (repoResponse.OperationStatusCode == 0)
                {
                    var listaDto = new List<SaleResponseDetailDto>();
                    foreach (var item in repoResponse.Data!)
                    {
                        var detailDto = new SaleResponseDetailDto
                        {
                            //Id = item.Id,
                            //SaleId = saleId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                           SalePrice = item.SalePrice,
                            LineTotal = item.LineTotal
                        };
                        listaDto.Add(detailDto);
                    }
                    return new ServiceResponse<IEnumerable<SaleResponseDetailDto>>
                    {
                        Data = listaDto,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Detalle obtenido exitosamente."
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50073:
                        return new ServiceResponse<IEnumerable<SaleResponseDetailDto>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };
                    default:
                        return new ServiceResponse<IEnumerable<SaleResponseDetailDto>>
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
                return new ServiceResponse<IEnumerable<SaleResponseDetailDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<List<SaleResponseDto>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var repoResponse = await _saleRepository.GetSaleByDateRangeAsync(startDate, endDate);

                if (repoResponse.OperationStatusCode == -1 || repoResponse.Data == null)
                {
                    return new ServiceResponse<List<SaleResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error al obtener las ventas"
                    };
                }

                if (repoResponse.OperationStatusCode == 1 || !repoResponse.Data.Any())
                {
                    return new ServiceResponse<List<SaleResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontraron ventas en el rango proporcionado"
                    };
                }

                var salesDtoList = repoResponse.Data.Select(transaction => new SaleResponseDto
                {
                    Id = transaction.Master.Id,
                    CustomerType = transaction.Master.CustomerType,
                    SaleDate = transaction.Master.SaleDate,
                    TotalAmount = transaction.Master.TotalAmount,
                    Details = transaction.Details.Select(d => new SaleResponseDetailDto
                    {
                        //SaleId = d.SaleId,
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        SalePrice = d.SalePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                }).ToList();

                return new ServiceResponse<List<SaleResponseDto>>
                {
                    Data = salesDtoList,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Compras obtenidas exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SaleResponseDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }

    }

}







