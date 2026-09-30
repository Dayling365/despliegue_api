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
using Microsoft.IdentityModel.Tokens;
using LibreriaDonCesar.DataAccess.Interfaces;
using Attribute = LibreriaDonCesar.Core.Entities.Attribute;

namespace LibreriaDonCesar.Business.Services
{
    public class AttributeService : IAttributeService
    {

        private readonly IAttributeRepository _attributeRepository;

        public AttributeService(IAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<Attribute>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _attributeRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Attribute>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Attribute>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Attribute>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Attribute>> GetByIdAsync(int id)
        {
            var repoResponse = await _attributeRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Attribute>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacio exitosa"
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50028:
                        return new ServiceResponse<Attribute>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe el atributo"
                        };

                    default:
                        return new ServiceResponse<Attribute>
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
                return new ServiceResponse<Attribute>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }


        public async Task<ServiceResponse<Attribute>> GetByNameAsync(string name)
        {
            var result = await _attributeRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Attribute>
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
                case 50007:
                    messageCode = MessageCodes.NotFound;
                    message = "No existe el atributo";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el atributo.";
                    break;


            }

            return new ServiceResponse<Attribute>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Attribute>> CreateAsync(CreateAttributeDto newAttribute)
        {
            try
            {
                var existingAttribute = await _attributeRepository.GetByNameAsync(newAttribute.AttributeName);


                if (existingAttribute.Data!.Id != 0 && !existingAttribute.Data.AttributeName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Attribute>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var attribute = new Attribute()
                {
                    AttributeName = newAttribute.AttributeName,

                };

                //llamado al metodo de repo
                var result = await _attributeRepository.AddAsync(attribute);

                return new ServiceResponse<Attribute>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Attribute>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Attribute>> UpdateAsync(int id, UpdateAttributeDto attribute)
        {

            try
            {
                var existingIdAttribute = await _attributeRepository.GetByIdAsync(id);

                if (existingIdAttribute.Data!.Id == 0 && existingIdAttribute.Data.AttributeName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Attribute>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe el atributo con el Id proporcionado"
                    };
                }

                var existingNameAttribute = await _attributeRepository.GetByNameAsync(attribute.AttributeName);
                if (existingNameAttribute.Data!.AttributeName != null && existingNameAttribute.Data.Id != id)
                {
                    return new ServiceResponse<Attribute>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un atributo con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataAttribute = new Attribute()
                {
                    AttributeName = attribute.AttributeName,
                    State = attribute.State,
                };

                //llamado al metodo de repo
                var result = await _attributeRepository.UpdateAsync(id, dataAttribute);

                return new ServiceResponse<Attribute>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Attribute>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }

        public async Task<ServiceResponse<Attribute>> SetStateAsync(int attributeId, bool state)
        {
            var response = new ServiceResponse<Attribute>();

            // Validar que el atributo exista
            var existingAttribute = await _attributeRepository.GetByIdAsync(attributeId);
            if (existingAttribute == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El atributo no existe";
                return response;
            }
            if (existingAttribute.Data.State == state)
            {
                response.Data = existingAttribute.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "La categoría ya está activada" : "La categoría ya está desactivada";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _attributeRepository.SetStateAsync(attributeId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado de la categoría";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Categoría activada" : "Categoría desactivada";

            return response;
        }
    }
}