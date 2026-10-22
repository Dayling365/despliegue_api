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
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        /// <summary>Listado de ventas (solo cabecera). El detalle: GET /api/Sales/{id} o /api/Sales/details/{saleId}.</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _saleService.GetAllSaleAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new SaleResponseDto
                {
                    Id = x.Master.Id,
                    CustomerTypeId = x.Master.CustomerTypeId,
                    CustomerTypeName = x.Master.CustomerTypeName,
                    SaleDate = x.Master.SaleDate,
                    TotalAmount = x.Master.TotalAmount,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<SaleResponseDto>>
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

            var serviceResponse = await _saleService.GetByIdAsync(id);
            if (serviceResponse.IsSuccess)
            {
                var sl = serviceResponse.Data!;
                var saleDto = new SaleResponseDto
                {
                    Id = sl.Master.Id,
                    CustomerTypeId = sl.Master.CustomerTypeId,
                    CustomerTypeName = sl.Master.CustomerTypeName,
                    SaleDate = sl.Master.SaleDate,
                    TotalAmount = sl.Master.TotalAmount,
                    Details = sl.Details.Select(d => new SaleResponseDetailDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        ColorName = d.ColorName,
                        PresentationName = d.PresentationName,
                        Quantity = d.Quantity,
                        SalePrice = d.SalePrice,
                        LineTotal = d.LineTotal
                    }).ToList(),
                };
                return Ok(new ApiResponse<SaleResponseDto>
                {
                    Data = saleDto,
                    Meta = new { message = serviceResponse.Message }
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro venta asociada al Id proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        /// <summary>Registra la venta (y su factura). Si algún producto queda en el umbral mínimo, la respuesta incluye "alerts".</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpPost]
        public async Task<IActionResult> Register(CreateSaleDto dto)
        {
            var serviceResponse = await _saleService.InsertAsync(dto);

            if (serviceResponse.IsSuccess)
            {
                var dataResponse = serviceResponse.Data!;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = dataResponse.Id },
                    dataResponse
                    );

            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo un conflicto" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("details/{saleId}")]
        public async Task<IActionResult> GetDetailById(int saleId)
        {
            if (saleId <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };

                return BadRequest(response);
            }

            var serviceResponse = await _saleService.GetDetailByIdAsync(saleId);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new ApiResponse<IEnumerable<SaleResponseDetailDto>>
                {
                    Data = serviceResponse.Data,
                    Meta = new { message = serviceResponse.Message }
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro un detalle asociado al Id proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("bydaterange")]
        public async Task<IActionResult> GetSaleByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] PaginationParams pagination)
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

            var serviceResponse = await _saleService.GetSaleByDateRangeAsync(startDate, endDate, pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new SaleResponseDto
                {
                    Id = x.Master.Id,
                    CustomerTypeId = x.Master.CustomerTypeId,
                    CustomerTypeName = x.Master.CustomerTypeName,
                    SaleDate = x.Master.SaleDate,
                    TotalAmount = x.Master.TotalAmount,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<SaleResponseDto>>
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
                    unsuccessfulResponse.Details = new { info = "No hay ventas en el rango indicado" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}
