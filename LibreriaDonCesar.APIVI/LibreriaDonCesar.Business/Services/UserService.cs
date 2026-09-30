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
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;


        //Constructor del Servicio 

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<User>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _userRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<User>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<User>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros de usuarios"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<User>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado al obtener los usuarios"
                    };
            }
        }

        public async Task<ServiceResponse<User>> GetByIdAsync(int id)
        {
            var repoResponse = await _userRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"

                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 50041:
                        return new ServiceResponse<User>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"

                        };


                    default:
                        return new ServiceResponse<User>
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
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                };


            }
        }

        public async Task<ServiceResponse<User>> GetByNameAsync(string name)
        {
            var result = await _userRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<User>
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
                case 50042:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontro el usuario con ese Name proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el usuario.";
                    break;
            }

            // Retorno final para los casos de error o no encontrado ////
            return new ServiceResponse<User>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<UserRole>> AssignRoleAsync(int userId, int roleId)
        {
            var repoResponse = await _userRepository.AssignRoleAsync(userId, roleId);

            if (repoResponse.OperationStatusCode == 0)
            {
                return new ServiceResponse<UserRole>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol asignado correctamente al usuario."
                };
            }
            switch (repoResponse.OperationStatusCode)
            {
                case 50070:
                    return new ServiceResponse<UserRole>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "  No existe el usuario con el id proporcionado."
                    };
                case 50071:
                    return new ServiceResponse<UserRole>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = " No existe el rol con el id proporcionado."
                    };
                case 50072:
                    return new ServiceResponse<UserRole>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "El usuario ya tiene asignado este rol."
                    };
                default:
                    return new ServiceResponse<UserRole>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado al asignar el rol."
                    };


            }
        }
        public async Task<ServiceResponse<User>> CreateAsync(CreateUserDto newUser)
        {
            try
            {

                var existingUser = await _userRepository.GetByNameAsync(newUser.UserName);

                if (existingUser.Data!.Id != 0 && !existingUser.Data.UserName.IsNullOrEmpty())
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,///*//
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }

                var User = new User()
                {
                    UserName = newUser.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    Email = newUser.Email,

                };

                //Llamando al metodo repo
                var result = await _userRepository.AddAsync(User);

                return new ServiceResponse<User>
                {

                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }


        public async Task<ServiceResponse<User>> UpdateAsync(int id, UpdateUserDto user)
        {
            try
            {

                var existingIdUser = await _userRepository.GetByIdAsync(id);
                if (existingIdUser.Data!.Id == 0 && existingIdUser.Data.UserName.IsNullOrEmpty())
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un usuario asociado al Id proporcionado"

                    };
                }


                var existingNameUser = await _userRepository.GetByNameAsync(user.UserName);
                if (existingNameUser.Data!.UserName != null && existingNameUser.Data.Id != id)
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,



                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe un usuario con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                var dataUser = new User()
                {
                    UserName = user.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
                    Email = user.Email,
                    State = user.State,
                };

                //llamando al metodo de repo
                var result = await _userRepository.UpdateAsync(id, dataUser);

                return new ServiceResponse<User>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }

        public async Task<ServiceResponse<User>> SetStateAsync(int userId, bool state)
        {
            var response = new ServiceResponse<User>();

            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El usuario no existe";
                return response;
            }

            if (existingUser.Data.State == state)
            {
                response.Data = existingUser.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "El usuario ya está activado" : "El usuario ya está desactivado";
                return response;
            }


            var repoResponse = await _userRepository.SetStateAsync(userId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del usuario";
                return response;
            }


            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Usuario activado" : "Usuario desactivado";

            return response;
        }


    }
}