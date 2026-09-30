

using DocumentFormat.OpenXml.Office2010.Excel;
 using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.Business.Services;
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
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryMovementService _inventoryMovementService;

        public InventoryMovementsController(IInventoryMovementService inventoryMovementService)
        {
            _inventoryMovementService = inventoryMovementService;
        }


        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _inventoryMovementService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoriesMovementDtoCollection = serviceResponse.Data.Data.Select(im => new InventoryMovementDto
                {
                    Id = im.Id,
                    DateTime = im.DateTime,
                    ProductId = im.ProductId,
                    MovementType = im.MovementType,
                    Quantity = im.Quantity, 
                    StockBefore = im.StockBefore,
                    StockAfter = im.StockAfter,
                    Reason = im.Reason,
                });

                var apiResponse = new ApiResponse<IEnumerable<InventoryMovementDto>>
                {
                    Data = inventoriesMovementDtoCollection,
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
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateInventoryMovementDto movementDto)
        {
            var serviceResponse = await _inventoryMovementService.CreateAsync(movementDto);

            if (serviceResponse.IsSuccess)
            {
                var newMovementDto = new InventoryMovementDto
                {
                    Id = serviceResponse.Data!.Id,
                    DateTime = serviceResponse.Data!.DateTime,
                    ProductId = serviceResponse.Data!.ProductId,
                    MovementType = serviceResponse.Data!.MovementType,
                    Quantity = serviceResponse.Data!.Quantity,
                    StockBefore = serviceResponse.Data!.StockBefore,
                    StockAfter = serviceResponse.Data!.StockAfter,
                    Reason = serviceResponse.Data!.Reason
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newMovementDto.Id },
                    newMovementDto
                );
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0 || id == null)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }
            var serviceResponse = await _inventoryMovementService.GetByIdAsync(id);


            if (serviceResponse.IsSuccess)
            {
                var movementDto = new InventoryMovementDto
                {
                    Id = serviceResponse.Data.Id,
                    DateTime = serviceResponse.Data.DateTime,
                    ProductId = serviceResponse.Data.ProductId,
                    MovementType = serviceResponse.Data.MovementType,
                    Quantity = serviceResponse.Data.Quantity,
                    StockBefore = serviceResponse.Data.StockBefore,
                    StockAfter = serviceResponse.Data.StockAfter,
                    Reason = serviceResponse.Data.Reason
                };
                return Ok(movementDto);

            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "404",
                        Message = "No se encontro un movimiento de inventario asociada al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };
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
        [HttpGet("product/{productId}")] 
        public async Task<IActionResult> GetMovementsByProductId(int productId)
        {
            // Validar que el id cumple con el formato esperado
            if (productId <= 0 || productId == null)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id de producto proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _inventoryMovementService.GetMovementsByProductIdAsync(productId);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la colección de movimientos recibida
                var movementsDtoList = serviceResponse.Data!.Select(movement => new InventoryMovementDto
                {
                    Id = movement.Id,
                    DateTime = movement.DateTime,
                    ProductId = movement.ProductId,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity, // Respetando tu ortografía "Quiantity"
                    StockBefore = movement.StockBefore,
                    StockAfter = movement.StockAfter,
                    Reason = movement.Reason
                }).ToList();

                return Ok(movementsDtoList);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "404",
                        Message = "No se encontraron movimientos de inventario de ese producto",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };
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

        [Authorize(Roles = "Administrador")]
        [HttpGet("bydaterange")]
        public async Task<IActionResult> GetMovementsByDateRangeAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default || endDate < startDate)
            {
                return BadRequest(new
                {
                    Code = "400",
                    Message = "Los parámetros proporcionados no son válidos",
                    Details = new { Error = "startDate y endDate deben ser válidos y endDate >= startDate" }
                });
            }

            var serviceResponse = await _inventoryMovementService.GetMovementsByDateRangeAsync(startDate, endDate);

            if (serviceResponse.IsSuccess)
                return Ok(serviceResponse.Data);

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };
                    return Ok(unsuccessfulResponse);
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Recurso no encontrado.";
                    unsuccessfulResponse.Details = serviceResponse.Details = new { info = serviceResponse.Message };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("type/{movementType}")] 
        public async Task<IActionResult> GetMovementsByType(string movementType)
        {
            // Validar que el tipo de movimiento no viaje vacío
            if (string.IsNullOrWhiteSpace(movementType))
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "El tipo de movimiento proporcionado no es válido",
                    Details = new { info = "El valor enviado no puede ser nulo o vacío" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _inventoryMovementService.GetMovementsByTypeAsync(movementType);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la colección de movimientos recibida
                var movementsDtoList = serviceResponse.Data!.Select(movement => new InventoryMovementDto
                {
                    Id = movement.Id,
                    DateTime = movement.DateTime,
                    ProductId = movement.ProductId,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity, // Respetando tu ortografía "Quiantity"
                    StockBefore = movement.StockBefore,
                    StockAfter = movement.StockAfter,
                    Reason = movement.Reason
                }).ToList();

                return Ok(movementsDtoList);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "404",
                        Message = "No se encontraron movimientos para el tipo proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };
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

    }
}
