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
    public class PurchaseService : IPurchaseService
    {
        // Inyección de dependencias
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUserService _userService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IUserService userService,
            ISupplierService supplierService, IProductService productService)
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
                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
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
                    case 0:
                    case 50050:
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
            catch (Exception)
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
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ColorName = item.ColorName,
                            PresentationName = item.PresentationName,
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
                        Message = "Detalle obtenido exitosamente."
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50050:
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
            catch (Exception)
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
                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    //Mapeo de la respuesta al Dto de respuesta
                    var dataResponse = new PurchaseResponseDto();

                    //Mapeo de la parte Maestra
                    dataResponse.Id = repoResponse.Data!.Master.Id;
                    dataResponse.SupplierId = repoResponse.Data!.Master.SupplierId;
                    dataResponse.SupplierName = repoResponse.Data!.Master.SupplierName;
                    dataResponse.PurchaseDate = repoResponse.Data!.Master.PurchaseDate;
                    dataResponse.TotalAmount = repoResponse.Data!.Master.TotalAmount;

                    //Mapeo de los detalles
                    dataResponse.Details = repoResponse.Data!.Details.Select(dt => new PurchaseResponseDetailDto
                    {
                        ProductId = dt.ProductId,
                        ProductName = dt.ProductName,
                        ColorName = dt.ColorName,
                        PresentationName = dt.PresentationName,
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

                switch (repoResponse.OperationStatusCode)
                {
                    case 50037:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "El detalle de la compra no puede estar vacío"
                        };

                    case 50050:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El proveedor indicado no existe"
                        };

                    case 50052:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "Uno o más productos no existen o están inactivos"
                        };

                    case 50062:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "La cantidad indicada no es válida"
                        };

                    case 50081:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "Hay productos repetidos en el detalle de la compra"
                        };

                    default:
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

        public async Task<ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination)
        {
            var result = await _purchaseRepository.GetPurchasesByDateRangeAsync(startDate, endDate, pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Compras obtenidas exitosamente"
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
                        Message = "No se encontraron compras en el rango proporcionado"
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
    }
}
