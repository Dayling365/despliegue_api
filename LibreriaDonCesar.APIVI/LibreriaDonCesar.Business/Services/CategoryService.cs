using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using LibreriaDonCesar.DataAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LibreriaDonCesar.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        //Constructor del Servicio 
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        //Programar la logica /reglas del negocio relacionadas a Categorias
        public async Task<ServiceResponse<PagedResponse<IEnumerable<Category>>>> GetAllAsync(PaginationParams pagination)
        {
            var result = await _categoryRepository.GetAllAsync(pagination);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<IEnumerable<Category>>>
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
                    return new ServiceResponse<PagedResponse<IEnumerable<Category>>>
                    {
                        Data = null,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<IEnumerable<Category>>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Category>> GetByIdAsync(int id)
        {
            var repoResponse = await _categoryRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode==0)
                {
                    return new ServiceResponse<Category>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"
                    };
                }
                switch (repoResponse.OperationStatusCode)
                {
                    case 50027:
                        return new ServiceResponse<Category>
                        {
                            Data = null,
                            IsSuccess = false,
                            //notfound
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No existe la Categoría"
                        };

                    default:
                        return new ServiceResponse<Category>
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
                return new ServiceResponse<Category>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"
                };

            }
        }

        public async Task<ServiceResponse<Category>> GetByNameAsync(string name)
        {
            var result = await _categoryRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Category>
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
                case 50027:
                    messageCode = MessageCodes.NotFound;
                    message = "No existe la Categoría";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener la Categoría.";
                    break;


            }

            return new ServiceResponse<Category>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<Category>> CreateAsync(CreateCategoryDto newCategory)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByNameAsync(newCategory.CategoryName);


                if (existingCategory.Data!.Id != 0 && !existingCategory.Data.CategoryName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Category>
                    {
                        Data = null,
                        IsSuccess = false,
                        //cambiar el error validation
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                //mapeo
                var category = new Category()
                {
                    CategoryName = newCategory.CategoryName,
                    Description = newCategory.Description
                };

                //llamado al metodo de repo
                var result = await _categoryRepository.AddAsync(category);

                return new ServiceResponse<Category>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Category>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                    
                };

            }
        }

        public async Task<ServiceResponse<Category>> UpdateAsync(int id, UpdateCategoryDto category)
        {

            try
            {

                //validar que la categoria exista segun Id
                var existingIdCategory = await _categoryRepository.GetByIdAsync(id);

                if (existingIdCategory.Data!.Id == 0 && existingIdCategory.Data.CategoryName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Category>
                    {
                        Data = null,
                        IsSuccess = false,
                        //notfound
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe categoria con el Id proporcionado"
                    };
                }

                //validar que el nombre de la categoria no coincida con otro nombre existente
                var existingNameCategory = await _categoryRepository.GetByNameAsync(category.CategoryName);
                if (existingNameCategory.Data!.CategoryName != null && existingNameCategory.Data.Id != id)
                {
                    return new ServiceResponse<Category>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una categoria con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                //mapeo
                var dataCategory = new Category()
                {
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    State = category.State,
                };

                //llamado al metodo de repo
                var result = await _categoryRepository.UpdateAsync(id, dataCategory);

                return new ServiceResponse<Category>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Category>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }
        public async Task<ServiceResponse<Category>> SetStateAsync(int categoryId, bool state)
        {
            var response = new ServiceResponse<Category>();

            // Validar que la categoría exista
            var existingCategory = await _categoryRepository.GetByIdAsync(categoryId);
            if (existingCategory == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "La categoría no existe";
                return response;
            }
            if (existingCategory.Data.State == state)
            {
                response.Data = existingCategory.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "La categoría ya está activada" : "La categoría ya está desactivada";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _categoryRepository.SetStateAsync(categoryId, state);

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
