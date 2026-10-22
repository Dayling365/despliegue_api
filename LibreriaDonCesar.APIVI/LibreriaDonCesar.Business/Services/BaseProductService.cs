using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Services
{
    public class BaseProductService : IBaseProductService
    {
        private readonly IBaseProductRepository _baseProductRepository;

        public BaseProductService(IBaseProductRepository baseProductRepository)
        {
            _baseProductRepository = baseProductRepository;
        }


        public async Task<ServiceResponse<PagedResponse<IEnumerable<BaseProduct>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _baseProductRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<BaseProduct>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<BaseProduct>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<BaseProduct>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<BaseProduct>> GetByIdAsync(int id)
        {
            var repoResponse = await _baseProductRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    return new ServiceResponse<BaseProduct>
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
                        return new ServiceResponse<BaseProduct>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<BaseProduct>
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
                return new ServiceResponse<BaseProduct>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };
            }
        }


        public async Task<ServiceResponse<BaseProduct>> CreateAsync(CreateBaseProductDto newBaseProduct)
        {
            try
            {
                if (newBaseProduct.CategoryId == 0)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la Categoría no es valido"
                    };
                }
                if (newBaseProduct.BrandId == 0)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la marca no es valida"
                    };
                }

                var baseProduct = new BaseProduct()
                {
                    ProductName = newBaseProduct.ProductName,
                    Description = newBaseProduct.Description,
                    CategoryId = newBaseProduct.CategoryId,
                    BrandId = newBaseProduct.BrandId,
                };

                var result = await _baseProductRepository.AddAsync(baseProduct);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Registro creado con exito",
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50020:
                        return new ServiceResponse<BaseProduct>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Ya existe un producto base con ese nombre y marca"
                        };

                    case 50050:
                        return new ServiceResponse<BaseProduct>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La categoría o la marca indicada no existe"
                        };

                    default:
                        return new ServiceResponse<BaseProduct>
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
                return new ServiceResponse<BaseProduct>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }



        public async Task<ServiceResponse<BaseProduct>> UpdateAsync(int id, UpdateBaseProductDto baseProduct)
        {
            try
            {
                if (baseProduct.CategoryId == 0)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la Categoría no es valido"
                    };
                }
                if (baseProduct.BrandId == 0)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la marca no es valida"
                    };
                }

                //mapeo
                var dataBaseProduct = new BaseProduct()
                {
                    ProductName = baseProduct.ProductName,
                    Description = baseProduct.Description,
                    CategoryId = baseProduct.CategoryId,
                    BrandId = baseProduct.BrandId,
                    State = baseProduct.State,
                };

                //llamando al metodo de repo
                var result = await _baseProductRepository.UpdateAsync(id, dataBaseProduct);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<BaseProduct>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Registro actualizado con exito",
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50020:
                        return new ServiceResponse<BaseProduct>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Ya existe otro producto base con ese nombre y marca"
                        };

                    case 50050:
                        return new ServiceResponse<BaseProduct>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No existe un producto base asociado al Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<BaseProduct>
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
                return new ServiceResponse<BaseProduct>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }


        public async Task<ServiceResponse<BaseProduct>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<BaseProduct>();

            var existingBaseProduct = await _baseProductRepository.GetByIdAsync(id);
            if (existingBaseProduct.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = existingBaseProduct.OperationStatusCode == 0 || existingBaseProduct.OperationStatusCode == 50050
                    ? MessageCodes.NotFound
                    : MessageCodes.ErrorDataBase;
                response.Message = existingBaseProduct.OperationStatusCode == 0 || existingBaseProduct.OperationStatusCode == 50050
                    ? "El producto base no existe"
                    : existingBaseProduct.Message ?? "Ocurrio un error inesperado";
                return response;
            }

            if (existingBaseProduct.Data.State == state)
            {
                response.Data = existingBaseProduct.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "El producto base ya está activado" : "El producto base ya está desactivado";
                return response;
            }

            var repoResponse = await _baseProductRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del producto base";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Producto base activado" : "Producto base desactivado";

            return response;
        }
    }
}
