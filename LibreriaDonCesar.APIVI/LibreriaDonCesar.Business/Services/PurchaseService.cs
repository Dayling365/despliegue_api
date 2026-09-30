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
    public class PurchaseService: IPurchaseService
    {
        // Inyección de dependencias
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IUserService _userService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IUserService userService,
            ISupplierService supplierService,IProductService productService)
        {
            _purchaseRepository = purchaseRepository;
            _userService = userService;
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllPurchasesAsync(PaginationParams pagination)

        {
            var result = await _purchaseRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }
        public async Task<ServiceResponse<PurchaseTransaction>> GetByIdAsync(int id)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetByIdAsync(id);
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<PurchaseTransaction>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Compra obtenida exitosamente."
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50037:
                        return new ServiceResponse<PurchaseTransaction>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };
                    default:
                        return new ServiceResponse<PurchaseTransaction>
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
                return new ServiceResponse<PurchaseTransaction>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>> GetDetailByIdAsync(int purchaseId)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetDetailByIdAsync(purchaseId);

                if (repoResponse.OperationStatusCode == 0)
                {
                    var listaDto = new List<PurchaseResponseDetailDto>();
                    foreach (var item in repoResponse.Data!)
                    {
                        var detailDto = new PurchaseResponseDetailDto
                        {
                            //PurchaseId = item.PurchaseId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            PurchasePrice = item.PurchasePrice,
                            LineTotal = item.LineTotal
                        };
                        listaDto.Add(detailDto);
                    }
                    return new ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>
                    {
                        Data = listaDto,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Detalle obtenida exitosamente."
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50037:
                        return new ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };
                    default:
                        return new ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>
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
                return new ServiceResponse<IEnumerable<PurchaseResponseDetailDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto)
        {
            try
            {
                // Validar que existe proveedor con el id
                var existentSupplier = await _supplierService.GetByIdAsync(dto.SupplierId);
                if (existentSupplier.Data! == null)
                {
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró el proveedor asociado al valor de Id proporcionado",

                    };
                }
              

                foreach (var detail in dto.Details)
                {
                    var existentProduct = await _productService.GetByIdAsync(detail.ProductId);
                    if (existentProduct.Data == null)
                    {
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"El producto con Id {detail.ProductId} no existe",

                        };
                    }
                }

                //Mapeo del Maestro de la compra
                var purchaseMaster = new Purchase
                {
                   
                    SupplierId = dto.SupplierId,
                    PurchaseDate = DateTime.Now,
                    TotalAmount = 0
                };

                //Mapeo del Dto a la clase base del Deretalle de la compra
                var purchaseDetails = dto.Details.Select(d => new PurchaseDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    PurchasePrice = d.PurchasePrice,
                }).ToList();

                //Invocamos al metodo del repositorio para insertar la compra
                var repoResponse = await _purchaseRepository.InsertAsync(purchaseMaster, purchaseDetails);

                //validar la respuesta del repositorio
                if (repoResponse.OperationStatusCode == 0)
                {
                    //Mapeo de la respuesta al Dto de respuesta
                    var dataResponse = new PurchaseResponseDto();

                    //Mapeo de la parte Maestra
                     dataResponse.Id = repoResponse.Data!.Master.Id;
                     dataResponse.SupplierId = repoResponse.Data!.Master.SupplierId;
                     dataResponse.PurchaseDate = repoResponse.Data!.Master.PurchaseDate;
                     dataResponse.TotalAmount = repoResponse.Data!.Master.TotalAmount;

                    //Mapeo de los detalles
                    dataResponse.Details = repoResponse.Data!.Details.Select(dt => new PurchaseResponseDetailDto
                    {
                        ProductId = dt.ProductId,
                        ProductName = dt.ProductName,
                        Quantity = dt.Quantity,
                        PurchasePrice = dt.PurchasePrice,
                        LineTotal = dt.LineTotal
                    }).ToList();

                    //retornamos la respuesta del servicio
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Compra registrada exitosamente."
                    };

                }
                else
                {
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Hubo un error al registrar la compra."
                    };
                }

            }
            catch (Exception)
            {
                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado"
                };
            }

        }

        public async Task<ServiceResponse<List<PurchaseResponseDto>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetPurchasesByDateRangeAsync(startDate, endDate);

                if (repoResponse.OperationStatusCode == -1 || repoResponse.Data == null)
                {
                    return new ServiceResponse<List<PurchaseResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error al obtener las compras"
                    };
                }

                if (repoResponse.OperationStatusCode == 1 || !repoResponse.Data.Any())
                {
                    return new ServiceResponse<List<PurchaseResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontraron compras en el rango proporcionado"
                    };
                }

                var salesDtoList = repoResponse.Data.Select(transaction => new PurchaseResponseDto
                {
                    Id = transaction.Master.Id,
                    SupplierId = transaction.Master.SupplierId,
                    PurchaseDate = transaction.Master.PurchaseDate,
                    TotalAmount = transaction.Master.TotalAmount,
                    Details = transaction.Details.Select(d => new PurchaseResponseDetailDto
                    {
                        //PurchaseId = d.PurchaseId,
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        PurchasePrice = d.PurchasePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                }).ToList();

                return new ServiceResponse<List<PurchaseResponseDto>>
                {
                    Data = salesDtoList,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Compras obtenidas exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<PurchaseResponseDto>>
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

