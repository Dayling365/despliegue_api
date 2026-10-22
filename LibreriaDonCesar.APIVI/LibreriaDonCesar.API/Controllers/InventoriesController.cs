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
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _inventoryService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ColorName = x.ColorName,
                    PresentationName = x.PresentationName,
                    SalePrice = x.SalePrice,
                    UnitsInStock = x.UnitsInStock,
                    MinStock = x.MinStock,
                    IsLowStock = x.IsLowStock,
                    StockStatus = x.StockStatus,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryDto>>
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

        /// <summary>Productos que están en el umbral mínimo o por debajo (los más críticos primero).</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("lowstock")]
        public async Task<IActionResult> GetLowStock([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _inventoryService.GetLowStockAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ColorName = x.ColorName,
                    PresentationName = x.PresentationName,
                    SalePrice = x.SalePrice,
                    UnitsInStock = x.UnitsInStock,
                    MinStock = x.MinStock,
                    IsLowStock = x.IsLowStock,
                    StockStatus = x.StockStatus,
                    UnitsMissing = x.UnitsMissing,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryDto>>
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
                    unsuccessfulResponse.Details = new { info = "No hay productos con stock bajo" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("product/{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            if (Id <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };

                return BadRequest(response);
            }

            var serviceResponse = await _inventoryService.GetByProductIdAsync(Id);

            if (serviceResponse.IsSuccess)
            {
                var inventoryDto = new InventoryDto
                {
                    Id = serviceResponse.Data!.Id,
                    ProductId = serviceResponse.Data!.ProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    ColorName = serviceResponse.Data!.ColorName,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    UnitsInStock = serviceResponse.Data!.UnitsInStock,
                    MinStock = serviceResponse.Data!.MinStock,
                    IsLowStock = serviceResponse.Data!.IsLowStock,
                    StockStatus = serviceResponse.Data!.StockStatus,
                };

                return Ok(inventoryDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro inventario asociado al Id del producto proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        /// <summary>Inventario de todas las variantes del producto con ese nombre.</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var badRequestResponse = new UnsuccessfulResponseDto();
                badRequestResponse.Code = "400";
                badRequestResponse.Message = "El dato proporcionado no es valido";
                badRequestResponse.Details = new { Error = "El name no puede ser nulo o vacio" };

                return BadRequest(badRequestResponse);
            }

            var serviceResponse = await _inventoryService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoriesDto = serviceResponse.Data.Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ColorName = i.ColorName,
                    PresentationName = i.PresentationName,
                    SalePrice = i.SalePrice,
                    UnitsInStock = i.UnitsInStock,
                    MinStock = i.MinStock,
                    IsLowStock = i.IsLowStock,
                    StockStatus = i.StockStatus,
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<InventoryDto>>
                {
                    Data = inventoriesDto,
                    Meta = new { message = serviceResponse.Message }
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No hay inventario asociado al valor name proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        /// <summary>Cambia el umbral mínimo (alerta de stock bajo) de un producto.</summary>
        [Authorize(Roles = "Administrador")]
        [HttpPatch("product/{Id}/minstock")]
        public async Task<IActionResult> UpdateMinStock(int Id, [FromBody] UpdateMinStockDto minStockDto)
        {
            if (Id <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };

                return BadRequest(response);
            }

            var serviceResponse = await _inventoryService.UpdateMinStockAsync(Id, minStockDto.MinStock);

            if (serviceResponse.IsSuccess)
            {
                var inventoryDto = new InventoryDto
                {
                    Id = serviceResponse.Data!.Id,
                    ProductId = serviceResponse.Data!.ProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    ColorName = serviceResponse.Data!.ColorName,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    UnitsInStock = serviceResponse.Data!.UnitsInStock,
                    MinStock = serviceResponse.Data!.MinStock,
                    IsLowStock = serviceResponse.Data!.IsLowStock,
                    StockStatus = serviceResponse.Data!.StockStatus,
                };

                return Ok(new ApiResponse<InventoryDto>
                {
                    Data = inventoryDto,
                    Meta = new { message = serviceResponse.Message }
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
