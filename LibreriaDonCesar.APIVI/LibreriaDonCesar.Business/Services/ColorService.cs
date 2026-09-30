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
using Color = LibreriaDonCesar.Core.Entities.Color;

namespace LibreriaDonCesar.Business.Services
{
    public class ColorService : IColorService
    {

        private readonly IColorRepository _colorRepository;

        public ColorService(IColorRepository colorRepository)
        {
            _colorRepository = colorRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<Color>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _colorRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Color>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Color>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Color>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Color>> GetByIdAsync(int id)
        {
            var repoResponse = await _colorRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Color>
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
                        return new ServiceResponse<Color>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe el color"
                        };

                    default:
                        return new ServiceResponse<Color>
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
                return new ServiceResponse<Color>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }


        public async Task<ServiceResponse<Color>> GetByNameAsync(string name)
        {
            var result = await _colorRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Color>
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
                    message = "No existe el color";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el color.";
                    break;


            }

            return new ServiceResponse<Color>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Color>> CreateAsync(CreateColorDto newColor)
        {
            try
            {
                var existingColor = await _colorRepository.GetByNameAsync(newColor.ColorName);


                if (existingColor.Data!.Id != 0 && !existingColor.Data.ColorName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Color>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var color = new Color()
                {
                    ColorName = newColor.ColorName,
                };

                //llamado al metodo de repo
                var result = await _colorRepository.AddAsync(color);

                return new ServiceResponse<Color>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Color>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Color>> UpdateAsync(int id, UpdateColorDto color)
        {

            try
            {
                var existingIdColor = await _colorRepository.GetByIdAsync(id);

                if (existingIdColor.Data!.Id == 0 && existingIdColor.Data.ColorName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Color>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe el color con el Id proporcionado"
                    };
                }

                var existingNameColor = await _colorRepository.GetByNameAsync(color.ColorName);
                if (existingNameColor.Data!.ColorName != null && existingNameColor.Data.Id != id)
                {
                    return new ServiceResponse<Color>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un color con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataColor = new Color()
                {
                    ColorName = color.ColorName,
                    State = color.State
                };

                //llamado al metodo de repo
                var result = await _colorRepository.UpdateAsync(id, dataColor);

                return new ServiceResponse<Color>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Color>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }
        public async Task<ServiceResponse<Color>> SetStateAsync(int id, bool state)
        {
            try
            {
                var result = await _colorRepository.SetStateAsync(id, state);

                if (result.Data == null)
                {
                    return new ServiceResponse<Color>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe el color con el Id proporcionado"
                    };
                }

                return new ServiceResponse<Color>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = state ? "Color activado con exito" : "Color desactivado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Color>
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
