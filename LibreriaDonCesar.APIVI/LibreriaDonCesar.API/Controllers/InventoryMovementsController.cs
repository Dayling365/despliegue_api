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
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryMovementService _movementService;

        public InventoryMovementsController(IInventoryMovementService movementService)
        {
            _movementService = movementService;
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _movementService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryMovementDto
                {
                    Id = x.Id,
                    DateTime = x.DateTime,
                    InventoryId = x.InventoryId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    MovementType = x.MovementType,
                    Quantity = x.Quantity,
                    StockBefore = x.StockBefore,
                    StockAfter = x.StockAfter,
                    PurchaseId = x.PurchaseId,
                    SaleId = x.SaleId,
                    Reason = x.Reason,
                    Alert = x.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = x.Alert.ProductId,
                        ProductName = x.Alert.ProductName,
                        UnitsInStock = x.Alert.UnitsInStock,
                        MinStock = x.Alert.MinStock,
                        StockStatus = x.Alert.StockStatus
                    },
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryMovementDto>>
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

        /// <summary>Entrada / Salida / Baja: Quantity = unidades que se mueven. Ajuste: Quantity = nuevo stock final.
        /// Si el stock queda en el umbral mínimo o por debajo, la respuesta incluye "alert".</summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateInventoryMovementDto movementDto)
        {
            var serviceResponse = await _movementService.CreateAsync(movementDto);

            if (serviceResponse.IsSuccess)
            {
                var newMovementDto = new InventoryMovementDto
                {
                    Id = serviceResponse.Data!.Id,
                    DateTime = serviceResponse.Data!.DateTime,
                    InventoryId = serviceResponse.Data!.InventoryId,
                    ProductId = serviceResponse.Data!.ProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    MovementType = serviceResponse.Data!.MovementType,
                    Quantity = serviceResponse.Data!.Quantity,
                    StockBefore = serviceResponse.Data!.StockBefore,
                    StockAfter = serviceResponse.Data!.StockAfter,
                    PurchaseId = serviceResponse.Data!.PurchaseId,
                    SaleId = serviceResponse.Data!.SaleId,
                    Reason = serviceResponse.Data!.Reason,
                    Alert = serviceResponse.Data!.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = serviceResponse.Data!.Alert.ProductId,
                        ProductName = serviceResponse.Data!.Alert.ProductName,
                        UnitsInStock = serviceResponse.Data!.Alert.UnitsInStock,
                        MinStock = serviceResponse.Data!.Alert.MinStock,
                        StockStatus = serviceResponse.Data!.Alert.StockStatus
                    },
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newMovementDto.Id },
                    newMovementDto
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

            var serviceResponse = await _movementService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var movementDto = new InventoryMovementDto
                {
                    Id = serviceResponse.Data!.Id,
                    DateTime = serviceResponse.Data!.DateTime,
                    InventoryId = serviceResponse.Data!.InventoryId,
                    ProductId = serviceResponse.Data!.ProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    MovementType = serviceResponse.Data!.MovementType,
                    Quantity = serviceResponse.Data!.Quantity,
                    StockBefore = serviceResponse.Data!.StockBefore,
                    StockAfter = serviceResponse.Data!.StockAfter,
                    PurchaseId = serviceResponse.Data!.PurchaseId,
                    SaleId = serviceResponse.Data!.SaleId,
                    Reason = serviceResponse.Data!.Reason,
                    Alert = serviceResponse.Data!.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = serviceResponse.Data!.Alert.ProductId,
                        ProductName = serviceResponse.Data!.Alert.ProductName,
                        UnitsInStock = serviceResponse.Data!.Alert.UnitsInStock,
                        MinStock = serviceResponse.Data!.Alert.MinStock,
                        StockStatus = serviceResponse.Data!.Alert.StockStatus
                    },
                };

                return Ok(movementDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro movimiento asociado al Id proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetMovementsByProductId(int productId, [FromQuery] PaginationParams pagination)
        {
            if (productId <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };

                return BadRequest(response);
            }

            var serviceResponse = await _movementService.GetMovementsByProductIdAsync(productId, pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryMovementDto
                {
                    Id = x.Id,
                    DateTime = x.DateTime,
                    InventoryId = x.InventoryId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    MovementType = x.MovementType,
                    Quantity = x.Quantity,
                    StockBefore = x.StockBefore,
                    StockAfter = x.StockAfter,
                    PurchaseId = x.PurchaseId,
                    SaleId = x.SaleId,
                    Reason = x.Reason,
                    Alert = x.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = x.Alert.ProductId,
                        ProductName = x.Alert.ProductName,
                        UnitsInStock = x.Alert.UnitsInStock,
                        MinStock = x.Alert.MinStock,
                        StockStatus = x.Alert.StockStatus
                    },
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryMovementDto>>
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
                    unsuccessfulResponse.Details = new { info = "El producto no tiene movimientos" };
                    return Ok(unsuccessfulResponse);

                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("bydaterange")]
        public async Task<IActionResult> GetMovementsByDateRangeAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] PaginationParams pagination)
        {
            if (startDate == default || endDate == default || endDate < startDate)
            {
                return BadRequest(new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Los parámetros proporcionados no son válidos",
                    Details = new { Error = "startDate y endDate deben ser válidos y endDate >= startDate" }
                });
            }

            var serviceResponse = await _movementService.GetMovementsByDateRangeAsync(startDate, endDate, pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryMovementDto
                {
                    Id = x.Id,
                    DateTime = x.DateTime,
                    InventoryId = x.InventoryId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    MovementType = x.MovementType,
                    Quantity = x.Quantity,
                    StockBefore = x.StockBefore,
                    StockAfter = x.StockAfter,
                    PurchaseId = x.PurchaseId,
                    SaleId = x.SaleId,
                    Reason = x.Reason,
                    Alert = x.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = x.Alert.ProductId,
                        ProductName = x.Alert.ProductName,
                        UnitsInStock = x.Alert.UnitsInStock,
                        MinStock = x.Alert.MinStock,
                        StockStatus = x.Alert.StockStatus
                    },
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryMovementDto>>
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
                    unsuccessfulResponse.Details = new { info = "No hay movimientos en el rango indicado" };
                    return Ok(unsuccessfulResponse);

                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("type/{movementType}")]
        public async Task<IActionResult> GetMovementsByType(string movementType, [FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _movementService.GetMovementsByTypeAsync(movementType, pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new InventoryMovementDto
                {
                    Id = x.Id,
                    DateTime = x.DateTime,
                    InventoryId = x.InventoryId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    MovementType = x.MovementType,
                    Quantity = x.Quantity,
                    StockBefore = x.StockBefore,
                    StockAfter = x.StockAfter,
                    PurchaseId = x.PurchaseId,
                    SaleId = x.SaleId,
                    Reason = x.Reason,
                    Alert = x.Alert == null ? null : new StockAlertDto
                    {
                        ProductId = x.Alert.ProductId,
                        ProductName = x.Alert.ProductName,
                        UnitsInStock = x.Alert.UnitsInStock,
                        MinStock = x.Alert.MinStock,
                        StockStatus = x.Alert.StockStatus
                    },
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<InventoryMovementDto>>
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
                    unsuccessfulResponse.Details = new { info = "No hay movimientos de ese tipo" };
                    return Ok(unsuccessfulResponse);

                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}
