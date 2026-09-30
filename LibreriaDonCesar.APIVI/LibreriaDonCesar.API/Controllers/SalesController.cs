using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
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

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _saleService.GetAllSaleAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                // 2. Mapeamos la colección desde serviceResponse.Data.Data respetando la estructura Maestro-Detalle
                var saleDtoCollection = serviceResponse.Data.Data.Select(sl => new SaleResponseDto
                {
                    Id = sl.Master.Id,
                    CustomerType = sl.Master.CustomerType,
                    SaleDate = sl.Master.SaleDate,
                    TotalAmount = sl.Master.TotalAmount,

                    Details = sl.Details.Select(d => new SaleResponseDetailDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        SalePrice = d.SalePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                }).ToList();

                // 3. Estructuramos la respuesta exitosa con los metadatos completos de paginación
                var apiResponse = new ApiResponse<IEnumerable<SaleResponseDto>>
                {
                    Data = saleDtoCollection,
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

            // 4. Manejo de respuestas no exitosas idéntico al de Usuarios
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
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var serviceResponse = await _saleService.GetByIdAsync(id);
            if (serviceResponse.IsSuccess)
            {
                var sl = serviceResponse.Data!;
                var saleDto = new SaleResponseDto
                {
                    Id = sl.Master.Id,
                    CustomerType = sl.Master.CustomerType,
                    SaleDate = sl.Master.SaleDate,
                    TotalAmount = sl.Master.TotalAmount,
                    Details = sl.Details.Select(d => new SaleResponseDetailDto
                    {
                        ProductId = d.ProductId,
                        ProductName=d.ProductName,
                        Quantity = d.Quantity,
                        SalePrice = d.SalePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
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
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Recurso no encontrado.";
                    unsuccessfulResponse.Details = serviceResponse.Details = new { info = serviceResponse.Message };
                    return NotFound(unsuccessfulResponse);
                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado.";
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

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
                case Core.Common.MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "Ocurrio un error en la validacion de datos";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(unSuccessfulResponse);
                case Core.Common.MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "Recurso no encontrado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message  };
                    return Conflict(unSuccessfulResponse);



                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };

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
                var saleDetailDto = new List<SaleResponseDetailDto>();
                foreach (var item in serviceResponse.Data)
                {
                    var detailDto = new SaleResponseDetailDto
                    {
                        //Id = item.Id,
                       //SaleId = saleId,
                        ProductId = item.ProductId,
                        ProductName= item.ProductName,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice,
                        LineTotal = item.LineTotal
                    };
                    saleDetailDto.Add(detailDto);
                }
                return Ok(saleDetailDto);
            }
            else
            {
                UnsuccessfulResponseDto unsuccessfulResponse;

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        unsuccessfulResponse = new UnsuccessfulResponseDto
                        {
                            Code = "404",
                            Message = "No se encontro un detalle asociada al Id proporcionado",
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

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("bydaterange")]
        public async Task<IActionResult> GetSaleByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
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

            var serviceResponse = await _saleService.GetSaleByDateRangeAsync(startDate, endDate);

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
                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "Stock insuficiente";
                    unsuccessfulResponse.Details = new { info = "Las productos solicitados superan la cantidad disponible en inventario" };
                    return Conflict(unsuccessfulResponse);


                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);

                
            }
        }




    }
}

    

