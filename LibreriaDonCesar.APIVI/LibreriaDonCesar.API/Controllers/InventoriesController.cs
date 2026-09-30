using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.Business.Services;
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

        //Constructor del controlador
        //Inyeccion de dependencia del ProductService
        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        //Endpoints
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _inventoryService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoriesDtoCollection = serviceResponse.Data.Data.Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    SalePrice = i.SalePrice,
                    UnitsInStock = i.UnitsInStock,
                    PurchasePrice = i.PurchasePrice,
                });

                var apiResponse = new ApiResponse<IEnumerable<InventoryDto>>
                {
                    Data = inventoriesDtoCollection,
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
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("product/{Id}")]

        public async Task<IActionResult> GetById(int Id)
        {
            //validar que el id cumple con el formato esperado
            if (Id <= 0 || Id == null)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }
            var serviceResponse = await _inventoryService.GetByIdAsync(Id);

            if (serviceResponse.IsSuccess)
            {
                //mapeo del Dto del Inventario recibida
                var inventoryDto = new InventoryDto
                {
                    Id = serviceResponse.Data.Id,
                    ProductId = serviceResponse.Data.ProductId,
                    ProductName = serviceResponse.Data.ProductName,
                    SalePrice = serviceResponse.Data.SalePrice,
                    UnitsInStock = serviceResponse.Data.UnitsInStock,
                    PurchasePrice = serviceResponse.Data.PurchasePrice,
                };

                return Ok(inventoryDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "404",
                        Message = "No se encontro un producto asociada al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };
                    //notfound
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "500",
                        Message = "Ocurrió un error",
                        Details = new { info = serviceResponse.Message ?? "Error interno no esperado" }
                    };
                    return StatusCode(500, unsuccessfulResponse);

            }
        }


        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            if (string.IsNullOrWhiteSpace(name))
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                unSuccessfulResponse.Details = new { Error = "El Name no puede ser nulo o vacío" };
                return BadRequest(unSuccessfulResponse);
            }

            var serviceResponse = await _inventoryService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var inventoryDto = new InventoryDto
                {
                    Id = serviceResponse.Data.Id,
                    ProductId = serviceResponse.Data.ProductId,
                    ProductName = serviceResponse.Data.ProductName,
                    SalePrice = serviceResponse.Data.SalePrice,
                    UnitsInStock = serviceResponse.Data.UnitsInStock,
                    PurchasePrice = serviceResponse.Data.PurchasePrice,

                };

                return Ok(inventoryDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró el producto en el inventario asociada al valor de Name proporcionado";
                    unSuccessfulResponse.Details = serviceResponse.Details = new { Error = "No hay registros asociados al valor de Name proporcionado" };
                    //componer
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}
