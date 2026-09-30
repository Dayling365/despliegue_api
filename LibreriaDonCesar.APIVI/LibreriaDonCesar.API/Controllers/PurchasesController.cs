using DocumentFormat.OpenXml.EMMA;
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
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _purchaseService.GetAllPurchasesAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                // 2. Mapeamos la colección desde serviceResponse.Data.Data respetando la estructura Maestro-Detalle
                var purchasesDtoCollection = serviceResponse.Data.Data.Select(pr => new PurchaseResponseDto
                {
                    Id = pr.Master.Id,
                    SupplierId = pr.Master.SupplierId,
                    PurchaseDate = pr.Master.PurchaseDate,
                    TotalAmount = pr.Master.TotalAmount,

                    Details = pr.Details.Select(d => new PurchaseResponseDetailDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        PurchasePrice = d.PurchasePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                }).ToList();

                // 3. Estructuramos la respuesta exitosa con los metadatos completos de paginación
                var apiResponse = new ApiResponse<IEnumerable<PurchaseResponseDto>>
                {
                    Data = purchasesDtoCollection,
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

        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var serviceResponse = await _purchaseService.GetByIdAsync(id);
            if (serviceResponse.IsSuccess)
            {
                var pr = serviceResponse.Data!;
                var purchaseDto = new PurchaseResponseDto
                {
                    Id = pr.Master.Id,
                    SupplierId = pr.Master.SupplierId,
                    PurchaseDate = pr.Master.PurchaseDate,
                    TotalAmount = pr.Master.TotalAmount,
                    Details = pr.Details.Select(d => new PurchaseResponseDetailDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        PurchasePrice = d.PurchasePrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                };
                return Ok(new ApiResponse<PurchaseResponseDto>
                {
                    Data = purchaseDto,
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

        [Authorize(Roles = "Administrador")]
        [HttpGet("details/{purchaseId}")]
        public async Task<IActionResult> GetDetailById(int purchaseId)
        {
            if (purchaseId <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _purchaseService.GetDetailByIdAsync(purchaseId);

            if (serviceResponse.IsSuccess)
            {
                var purchaseDetailDto = new List<PurchaseResponseDetailDto>();
                foreach (var item in serviceResponse.Data)
                {
                    var detailDto = new PurchaseResponseDetailDto
                    {
                        //PurchaseId = item.PurchaseId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        PurchasePrice = item.PurchasePrice,
                        LineTotal = item.LineTotal
                    };
                    purchaseDetailDto.Add(detailDto);
                }
                return Ok(purchaseDetailDto);
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
                            Message = "No se encontro un producto asociada al Id proporcionado",
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

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Register(CreatePurchaseDto dto)
        {
            var serviceResponse = await _purchaseService.InsertAsync(dto);

            if (serviceResponse.IsSuccess)
            {
                var dataResponse = serviceResponse.Data!;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = dataResponse.Id },
                    dataResponse
                );
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "Ocurrio un error en la validacion de datos";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message };

                    return BadRequest(unsuccessfulResponse);
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message =  "Recurso no encontrado.";
                    unsuccessfulResponse.Details = serviceResponse.Details = new { info = serviceResponse.Message };
                    return NotFound(unsuccessfulResponse);
                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado.";
                    return StatusCode(500, unsuccessfulResponse);

                


            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("bydaterange")]
        public async Task<IActionResult> GetPurchasesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
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

            var serviceResponse = await _purchaseService.GetPurchasesByDateRangeAsync(startDate, endDate);

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

    }
}
   