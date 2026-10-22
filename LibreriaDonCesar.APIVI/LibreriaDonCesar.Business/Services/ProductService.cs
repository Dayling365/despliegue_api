using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using LibreriaDonCesar.DataAccess.Repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }


        public async Task<ServiceResponse<PagedResponse<IEnumerable<Product>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _productRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Product>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Product>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Product>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Product>> GetByIdAsync(int id)
        {
            var repoResponse = await _productRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    return new ServiceResponse<Product>
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
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"

                        };


                    default:
                        return new ServiceResponse<Product>
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
                return new ServiceResponse<Product>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                };


            }
        }


        // Devuelve TODAS las variantes (color / presentación) del producto con ese nombre
        public async Task<ServiceResponse<IEnumerable<Product>>> GetByNameAsync(string name)
        {
            var result = await _productRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0 && result.Data != null && result.Data.Any())
            {
                return new ServiceResponse<IEnumerable<Product>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 0:
                case 50051:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontro producto con el Name proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el producto.";
                    break;
            }


            return new ServiceResponse<IEnumerable<Product>>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Product>> CreateAsync(CreateProductDto newProduct)
        {
            try
            {
                if (newProduct.BaseProductId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id del producto base no es valido"
                    };
                }
                if (newProduct.PresentationId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la Presentación no es valido"
                    };
                }
                if (newProduct.ColorId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id del color no es valido"
                    };
                }



                var product = new Product()
                {
                    BaseProductId = newProduct.BaseProductId,
                    ColorId = newProduct.ColorId,
                    PresentationId = newProduct.PresentationId,
                    SalePrice = newProduct.SalePrice,
                    MinStock = newProduct.MinStock,
                };

                var result = await _productRepository.AddAsync(product);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<Product>
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
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Ya existe una variante con ese producto base, color y presentación"
                        };

                    case 50050:
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El producto base, el color o la presentación indicada no existe"
                        };

                    case 50082:
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = "El stock mínimo no puede ser negativo"
                        };

                    default:
                        return new ServiceResponse<Product>
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
                return new ServiceResponse<Product>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }



        public async Task<ServiceResponse<Product>> UpdateAsync(int id, UpdateProductDto product)
        {
            try
            {
                //validar que el producto exista segun Id
                var existingIdProduct = await _productRepository.GetByIdAsync(id);
                if (existingIdProduct.OperationStatusCode == 50050 || (existingIdProduct.OperationStatusCode == 0 && existingIdProduct.Data == null))
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un producto asociado al Id proporcionado"

                    };
                }

                if (product.BaseProductId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id del producto base no es valido"
                    };
                }
                if (product.PresentationId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la Presentación no es valido"
                    };
                }
                if (product.ColorId == 0)
                {
                    return new ServiceResponse<Product>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id del color no es valido"
                    };
                }
                //mapeo
                var dataProduct = new Product()
                {
                    BaseProductId = product.BaseProductId,
                    ColorId = product.ColorId,
                    PresentationId = product.PresentationId,
                    SalePrice = product.SalePrice,
                    State = product.State,
                };


                //llamando al metodo de repo
                var result = await _productRepository.UpdateAsync(id, dataProduct);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<Product>
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
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Ya existe otra variante con ese producto base, color y presentación"
                        };

                    case 50050:
                        return new ServiceResponse<Product>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No existe un producto asociado al Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<Product>
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
                return new ServiceResponse<Product>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }





        public async Task<ServiceResponse<Product>> SetStateAsync(int productId, bool state)
        {
            var response = new ServiceResponse<Product>();

            var existingProduct = await _productRepository.GetByIdAsync(productId);
            if (existingProduct.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = existingProduct.OperationStatusCode == 0 || existingProduct.OperationStatusCode == 50050
                    ? MessageCodes.NotFound
                    : MessageCodes.ErrorDataBase;
                response.Message = existingProduct.OperationStatusCode == 0 || existingProduct.OperationStatusCode == 50050
                    ? "El producto no existe"
                    : existingProduct.Message ?? "Ocurrio un error inesperado";
                return response;
            }

            if (existingProduct.Data.State == state)
            {
                response.Data = existingProduct.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "El producto ya está activado" : "El producto ya está desactivado";
                return response;
            }

            var repoResponse = await _productRepository.SetStateAsync(productId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del producto";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Producto activado" : "Producto desactivado";

            return response;
        }


    }
}
