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

namespace LibreriaDonCesar.Business.Services
{
   public class UnitMeasureService: IUnitMeasureService
    {

        private readonly IUnitMeasureRepository _unitMeasureRepository;

        public UnitMeasureService(IUnitMeasureRepository unitMeasureRepository)
        {
            _unitMeasureRepository = unitMeasureRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<UnitMeasure>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _unitMeasureRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<UnitMeasure>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<UnitMeasure>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<UnitMeasure>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }



        public async Task<ServiceResponse<UnitMeasure>> GetByIdAsync(int id)
        {
            var repoResponse = await _unitMeasureRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<UnitMeasure>
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
                        return new ServiceResponse<UnitMeasure>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe la unidad de medida"
                        };

                    default:
                        return new ServiceResponse<UnitMeasure>
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
                return new ServiceResponse<UnitMeasure>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }


        public async Task<ServiceResponse<UnitMeasure>> GetByNameAsync(string name)
        {
            var result = await _unitMeasureRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<UnitMeasure>
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
                    message = "No existe la unidad de medida";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la unidad de medida.";
                    break;


            }

            return new ServiceResponse<UnitMeasure>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<UnitMeasure>> CreateAsync(CreateUnitMeasureDto newUnitMeasure)
        {
            try
            {
                var existingUnitMeasure = await _unitMeasureRepository.GetByNameAsync(newUnitMeasure.UnitMeasureName);


                if (existingUnitMeasure.Data!.Id != 0 && !existingUnitMeasure.Data.UnitMeasureName.IsNullOrEmpty())
                {
                    return new ServiceResponse<UnitMeasure>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var unitMeasure = new UnitMeasure()
                {
                    UnitMeasureName = newUnitMeasure.UnitMeasureName,
                    State = true

                };

                //llamado al metodo de repo
                var result = await _unitMeasureRepository.AddAsync(unitMeasure);

                return new ServiceResponse<UnitMeasure>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<UnitMeasure>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<UnitMeasure>> UpdateAsync(int id, UpdateUnitMeasureDto unitMeasure)
        {

            try
            {

                //validar que la categoria exista segun Id
                var existingIdUnitMeasure = await _unitMeasureRepository.GetByIdAsync(id);

                if (existingIdUnitMeasure.Data!.Id == 0 && existingIdUnitMeasure.Data.UnitMeasureName.IsNullOrEmpty())
                {
                    return new ServiceResponse<UnitMeasure>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe la unidad de medida con el Id proporcionado"
                    };
                }

                //validar que el nombre de la categoria no coincida con otro nombre existente
                var existingNameUnitMeasure = await _unitMeasureRepository.GetByNameAsync(unitMeasure.UnitMeasureName);
                if (existingNameUnitMeasure.Data!.UnitMeasureName != null && existingNameUnitMeasure.Data.Id != id)
                {
                    return new ServiceResponse<UnitMeasure>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una unidad de medida  con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataUnitMeasure = new UnitMeasure()
                {
                    UnitMeasureName = unitMeasure.UnitMeasureName,
                    State = unitMeasure.State,
                };

                //llamado al metodo de repo
                var result = await _unitMeasureRepository.UpdateAsync(id, dataUnitMeasure);

                return new ServiceResponse<UnitMeasure>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<UnitMeasure>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }
        public async Task<ServiceResponse<UnitMeasure>> SetStateAsync(int unitMeasureId, bool state)
        {
            var response = new ServiceResponse<UnitMeasure>();

            var existingUnitMeasure = await _unitMeasureRepository.GetByIdAsync(unitMeasureId);
            if (existingUnitMeasure == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = " La unidad de medida no existe";
                return response;
            }
            if (existingUnitMeasure.Data.State == state)
            {
                response.Data = existingUnitMeasure.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "La unidad de medida ya está activada" : "La unidad de medida ya está desactivada";
                return response;
            }

            var repoResponse = await _unitMeasureRepository.SetStateAsync(unitMeasureId,state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado de la unidad de medida";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "unidad de medida activada" : "unidad de medida desactivada";

            return response;
        }



    }
}
