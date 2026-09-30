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
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }



        public async Task<ServiceResponse<PagedResponse<IEnumerable<Supplier>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _supplierRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Supplier>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Supplier>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Supplier>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }


        public async Task<ServiceResponse<Supplier>> GetByIdAsync(int id)
        {
            var repoResponse = await _supplierRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Supplier>
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
                        return new ServiceResponse<Supplier>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No hay registros disponibles con el Id proporcionado"
                        };

                    default:
                        return new ServiceResponse<Supplier>
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
                return new ServiceResponse<Supplier>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<Supplier>> GetByNameAsync(string name)
        {
            var result = await _supplierRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Supplier>
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
                    message = "No se encontraron registros con el Name proporcionado";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el proveedor.";
                    break;


            }

            return new ServiceResponse<Supplier>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Supplier>> CreateAsync(CreateSupplierDto newSupplier)
        {
            try
            {
                var existingSupplier = await _supplierRepository.GetByNameAsync(newSupplier.SupplierName);


                if (existingSupplier.Data!.Id != 0 && !existingSupplier.Data.SupplierName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Supplier>
                    {
                        Data = null,
                        IsSuccess = false,
                        //conflict
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var supplier = new Supplier()
                {
                    SupplierName = newSupplier.SupplierName,
                    Email = newSupplier.Email,
                    State = true

                };

                //llamado al metodo de repo
                var result = await _supplierRepository.AddAsync(supplier);

                return new ServiceResponse<Supplier>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Supplier>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",

                };

            }
        }

        public async Task<ServiceResponse<Supplier>> UpdateAsync(int id, UpdateSupplierDto supplier)
        {

            try
            {

                //validar que la categoria exista segun Id
                var existingIdSupplier = await _supplierRepository.GetByIdAsync(id);

                if (existingIdSupplier.Data!.Id == 0 && existingIdSupplier.Data.SupplierName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Supplier>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe proveedor con el Id proporcionado"
                    };
                }

                //validar que el nombre de la categoria no coincida con otro nombre existente
                var existingNameSupplier = await _supplierRepository.GetByNameAsync(supplier.SupplierName);
                if (existingNameSupplier.Data!.SupplierName != null && existingNameSupplier.Data.Id != id)
                {
                    return new ServiceResponse<Supplier>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un proveedor con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataSupplier = new Supplier()
                {
                    SupplierName = supplier.SupplierName,
                    Email = supplier.Email,
                    State = supplier.State,
                };

                //llamado al metodo de repo
                var result = await _supplierRepository.UpdateAsync(id, dataSupplier);

                return new ServiceResponse<Supplier>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Supplier>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }
        public async Task<ServiceResponse<Supplier>> SetStateAsync(int supplierId, bool state)
        {
            var response = new ServiceResponse<Supplier>();

            var existingSupplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (existingSupplier == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El proveedor no existe";
                return response;
            }
            if (existingSupplier.Data.State == state)
            {
                response.Data = existingSupplier.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "El proveedor ya está activado" : "El proveedor ya está desactivado";
                return response;
            }

            var repoResponse = await _supplierRepository.SetStateAsync(supplierId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del proveedor";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Proveedor activado" : "Proveedor desactivado";

            return response;
        }

    }
}
