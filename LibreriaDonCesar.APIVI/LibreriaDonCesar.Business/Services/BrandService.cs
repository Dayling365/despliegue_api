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
using Microsoft.Extensions.Configuration;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LibreriaDonCesar.Business.Services
{
    public class BrandService : IBrandService
    {

        private readonly IBrandRepository _brandRepository;

        public BrandService(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<Brand>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _brandRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Brand>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Brand>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Brand>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }
        public async Task<ServiceResponse<Brand>> GetByIdAsync(int id)
        {
            var repoResponse = await _brandRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Brand>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacio exitosa"
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50050:
                        return new ServiceResponse<Brand>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe la marca"
                        };

                    default:
                        return new ServiceResponse<Brand>
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
                return new ServiceResponse<Brand>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }


        public async Task<ServiceResponse<Brand>> GetByNameAsync(string name)
        {
            var result = await _brandRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Brand>
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
                case 50051:
                    messageCode = MessageCodes.NotFound;
                    message = "No existe la marca";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la marca.";
                    break;


            }

            return new ServiceResponse<Brand>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Brand>> CreateAsync(CreateBrandDto newBrand)
        {
            try
            {
                var existingBrand = await _brandRepository.GetByNameAsync(newBrand.BrandName);


                if (existingBrand.Data!.Id != 0 && !existingBrand.Data.BrandName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Brand>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var brand = new Brand()
                {
                    BrandName = newBrand.BrandName,

                };

                //llamado al metodo de repo
                var result = await _brandRepository.AddAsync(brand);

                return new ServiceResponse<Brand>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Brand>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Brand>> UpdateAsync(int id, UpdateBrandDto brand)
        {

            try
            {
                var existingIdBrand = await _brandRepository.GetByIdAsync(id);

                if (existingIdBrand.Data!.Id == 0 && existingIdBrand.Data.BrandName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Brand>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe la marca con el Id proporcionado"
                    };
                }

                var existingNameBrand = await _brandRepository.GetByNameAsync(brand.BrandName);
                if (existingNameBrand.Data!.BrandName != null && existingNameBrand.Data.Id != id)
                {
                    return new ServiceResponse<Brand>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una marca con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataBrand = new Brand()
                {
                    BrandName = brand.BrandName,
                    State = brand.State,
                };

                //llamado al metodo de repo
                var result = await _brandRepository.UpdateAsync(id, dataBrand);

                return new ServiceResponse<Brand>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Brand>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }

        public async Task<ServiceResponse<Brand>> SetStateAsync(int brandId, bool state)
        {
            var response = new ServiceResponse<Brand>();

            // Validar que la marca exista
            var existingBrand = await _brandRepository.GetByIdAsync(brandId);
            if (existingBrand == null || existingBrand.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "La marca no existe";
                return response;
            }
            if (existingBrand.Data.State == state)
            {
                response.Data = existingBrand.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "La marca ya está activada" : "La marca ya está desactivada";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _brandRepository.SetStateAsync(brandId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado de la marca";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Marca activada" : "Marca desactivada";

            return response;
        }


    }
}
