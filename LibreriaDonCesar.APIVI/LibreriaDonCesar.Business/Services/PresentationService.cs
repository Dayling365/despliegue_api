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
    public class PresentationService : IPresentationService
    {
        private readonly IPresentationRepository _presentationRepository;

        public PresentationService(IPresentationRepository presentationRepository)
        {
            _presentationRepository = presentationRepository;
        }
        public async Task<ServiceResponse<PagedResponse<IEnumerable<Presentation>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _presentationRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Presentation>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Presentation>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Presentation>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Presentation>> GetByIdAsync(int id)
        {
            var repoResponse = await _presentationRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode==0)
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50050:
                        return new ServiceResponse<Presentation>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro al Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<Presentation>
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
                return new ServiceResponse<Presentation>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<Presentation>> GetByNameAsync(string name)
        {
            var result = await _presentationRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Presentation>
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
                    message = "No se encontro registro con el Name proporcionado";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la presentación.";
                    break;


            }
            return new ServiceResponse<Presentation>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<Presentation>> CreateAsync(CreatePresentationDto newPresentation)
        {
            try
            {
                var existingPresentation = await _presentationRepository.GetByNameAsync(newPresentation.PresentationName);


                if (existingPresentation.Data!.Id != 0 && !existingPresentation.Data.PresentationName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el Name proporcionado"
                    };
                }

                //mapeo
                var presentation = new Presentation()
                {
                    PresentationName = newPresentation.PresentationName,
                    Amount = newPresentation.Amount,
                    UnitMeasureId = newPresentation.UnitMeasureId,
                };

                if (presentation.UnitMeasureId == 0)
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la unidad de medida no es valido"
                    };
                }

                //llamado al metodo de repo
                var result = await _presentationRepository.AddAsync(presentation);

                return new ServiceResponse<Presentation>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentation>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Presentation>> UpdateAsync(int id, UpdatePresentationDto presentation)
        {

            try
            {
                var existingIdCustomer = await _presentationRepository.GetByIdAsync(id);

                if (existingIdCustomer.Data!.Id == 0 && existingIdCustomer.Data.PresentationName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe presentacion con el Id proporcionado"
                    };
                }

                

                //validar que el nombre de la categoria no coincida con otro nombre existente
                var existingNameCategory = await _presentationRepository.GetByNameAsync(presentation.PresentationName);
                if (existingNameCategory.Data!.PresentationName != null && existingNameCategory.Data.Id != id)
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una presentacion con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }
                if(presentation.UnitMeasureId == 0)
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la unidad de medida no puede ser 0"
                    };
                }
                if (presentation.UnitMeasureId == 0)
                {
                    return new ServiceResponse<Presentation>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "El Id de la unidad de medida no es valido"
                    };
                }
               
                //mapeo
                var dataCustomer = new Presentation()
                {
                     PresentationName = presentation.PresentationName,
                    Amount = presentation.Amount,
                    UnitMeasureId = presentation.UnitMeasureId,
                };

                //llamado al metodo de repo
                var result = await _presentationRepository.UpdateAsync(id, dataCustomer);

                return new ServiceResponse<Presentation>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentation>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }

        public async Task<ServiceResponse<Presentation>> SetStateAsync(int presentationId, bool state)
        {
            var response = new ServiceResponse<Presentation>();

            // Validar que la categoría exista
            var existingPresentation = await _presentationRepository.GetByIdAsync(presentationId);
            if (existingPresentation == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "La presentacion no existe";
                return response;
            }
            if (existingPresentation.Data.State == state)
            {
                response.Data = existingPresentation.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "La presentación ya está activada" : "La presentación ya está desactivada";
                return response;
            }


            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _presentationRepository.SetStateAsync(presentationId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado de la presentacion";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Presentacion activada" : "Presentacion desactivada";

            return response;
        }
    }
}
