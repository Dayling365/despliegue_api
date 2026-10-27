using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibreriaDonCesar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public AlertsController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>Alertas de stock bajo: productos en el umbral mínimo o por debajo (los más críticos primero).</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("stock")]
        public async Task<IActionResult> GetStockAlerts([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _inventoryService.GetLowStockAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new StockAlertDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    UnitsInStock = x.UnitsInStock,
                    MinStock = x.MinStock,
                    StockStatus = x.StockStatus
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<StockAlertDto>>
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

        /// <summary>Cantidad de alertas activas (para mostrar un badge en el frontend).</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var serviceResponse = await _inventoryService.GetLowStockAsync(new PaginationParams { PageNumber = 1, PageSize = 1 });

            if (serviceResponse.IsSuccess)
            {
                var total = serviceResponse.Data?.TotalRecords ?? 0;
                return Ok(new ApiResponse<object> { Data = new { count = total } });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto
            {
                Code = "500",
                Message = "Ocurrio un error inesperado",
                Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" }
            };
            return StatusCode(500, unsuccessfulResponse);
        }
    }
}
