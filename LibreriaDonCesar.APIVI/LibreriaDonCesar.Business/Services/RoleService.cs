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
using Microsoft.IdentityModel.Tokens;

namespace LibreriaDonCesar.Business.Services
{
    public class RoleService: IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ServiceResponse<PagedResponse<IEnumerable<Role>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _roleRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Role>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Role>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Role>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Role>> GetByIdAsync(int id)
        {
            var repoResponse = await _roleRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Role>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50040:
                        return new ServiceResponse<Role>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe el rol"
                        };

                    default:
                        return new ServiceResponse<Role>
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
                return new ServiceResponse<Role>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<Role>> GetByNameAsync(string name)
        {
            var result = await _roleRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Role>
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
                case 50008:
                    messageCode = MessageCodes.NotFound;
                    message = "No existe el rol";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el rol.";
                    break;


            }

            return new ServiceResponse<Role>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<Role>> CreateAsync(CreateRoleDto newRole)
        {
            try
            {
                var existingRole = await _roleRepository.GetByNameAsync(newRole.RoleName);


                if (existingRole.Data!.Id != 0 && !existingRole.Data.RoleName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Role>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var role = new Role()
                {
                    RoleName = newRole.RoleName
                };

                //llamado al metodo de repo
                var result = await _roleRepository.AddAsync(role);

                return new ServiceResponse<Role>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Role>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Role>> UpdateAsync(int id, UpdateRoleDto role)
        {

            try
            {

                //validar que el rol exista segun Id
                var existingIdRole = await _roleRepository.GetByIdAsync(id);

                if (existingIdRole.Data!.Id == 0 && existingIdRole.Data.RoleName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Role>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe el rol con el Id proporcionado"
                    };
                }

                //validar que el nombre del rol no coincida con otro nombre existente
                var existingNameRole = await _roleRepository.GetByNameAsync(role.RoleName);

                if (existingNameRole.Data!.RoleName != null && existingNameRole.Data.Id != id)
                {
                    return new ServiceResponse<Role>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un rol con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataRole = new  Role()
                {
                    RoleName = role.RoleName,
                    State = role.State,
                };

                //llamado al metodo de repo
                var result = await _roleRepository.UpdateAsync(id, dataRole);

                return new ServiceResponse<Role>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Role>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }
        public async Task<ServiceResponse<Role>> SetStateAsync(int roleId, bool state)
        {
            var response = new ServiceResponse<Role>();

            // Validar que la categoría exista
            var existingRole = await _roleRepository.GetByIdAsync(roleId);
            if (existingRole == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El rol no existe";
                return response;
            }

            if (existingRole.Data.State == state)
            {
                response.Data = existingRole.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ?"El rol ya esta activado":"El rol ya esta desactivado ";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _roleRepository.SetStateAsync(roleId, state);

 
            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del rol";
                return response;
            }

           

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Rol activado" : "Rol desactivado";

            return response;
        }




    }
}
