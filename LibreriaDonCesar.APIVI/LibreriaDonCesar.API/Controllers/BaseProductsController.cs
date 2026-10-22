using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibreriaDonCesar.API.Controllers
{
    /// <summary>Productos base: lo común a todas las variantes (nombre, descripción, categoría y marca). Las variantes se administran en /api/Products.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseProductsController : ControllerBase
    {
        private readonly IBaseProductService _baseProductService;

        public BaseProductsController(IBaseProductService baseProductService)
        {
            _baseProductService = baseProductService;
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _baseProductService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new BaseProductDto
                {
                    Id = x.Id,
                    ProductName = x.ProductName,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    BrandId = x.BrandId,
                    BrandName = x.BrandName,
                    State = x.State,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<BaseProductDto>>
                {
                    Data = items,
                    Meta = new
                    {
                        totalRecords = serviceResponse.Data.TotalRecords,
                        totalPages = serviceResponse.Data.TotalPages,
                        pageNumber = serviceResponse.Data.PageNumber,
                        pageSize = serviceResponse.Data.PageSize,
                        message = serviceResponse.Message
                    }
                };

                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };

                return BadRequest(response);
            }

            var serviceResponse = await _baseProductService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var baseProductDto = new BaseProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    State = serviceResponse.Data!.State,
                };

                return Ok(baseProductDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro producto base asociado al Id proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateBaseProductDto baseProductDto)
        {
            var serviceResponse = await _baseProductService.CreateAsync(baseProductDto);

            if (serviceResponse.IsSuccess)
            {
                var newBaseProductDto = new BaseProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    State = serviceResponse.Data!.State,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newBaseProductDto.Id },
                    newBaseProductDto
                    );
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo un conflicto" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBaseProductDto dataBaseProduct)
        {
            var serviceResponse = await _baseProductService.UpdateAsync(id, dataBaseProduct);

            if (serviceResponse.IsSuccess)
            {
                var updatedBaseProduct = new BaseProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    State = serviceResponse.Data!.State,
                };

                return Ok(updatedBaseProduct);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo un conflicto" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _baseProductService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    Id = serviceResponse.Data!.Id,
                    ProductName = serviceResponse.Data!.ProductName,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    BrandId = serviceResponse.Data!.BrandId,
                    State = serviceResponse.Data!.State,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}
