using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class InvoiceController : ControllerBase
    {
        //inyeccion del servicio
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoice)
        {
            _invoiceService = invoice;
        }

        [Authorize(Roles ="Administrador, Vendedor")]
        [HttpGet("PrintQueue")]
        public async Task<IActionResult> PrintQueue()
        {
            var serviceResponse = await _invoiceService.InvoiceQueue();
            if (serviceResponse.IsSuccess)
            {
                var response = new ApiResponse<List<Invoice>>
                {
                    Data = serviceResponse.Data,
                    Meta = new { TotalElements = serviceResponse.Data!.Count }
                };
                return Ok(response);
            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {

                case MessageCodes.NoData:
                    unSuccessfulResponse.Code = "200";
                    unSuccessfulResponse.Message = "No se encontraron Registros";
                    unSuccessfulResponse.Details = new { info = "No hay facturas que imprimir" };

                    return Ok(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado ";
                    unSuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };

                    return StatusCode(500, unSuccessfulResponse);
            }

        }
        
        [Authorize(Roles = "Administrador, Vendedor")]
        [HttpGet("ToPrint")]
        public async Task<IActionResult> ToPrint()
        {
            var serviceResponse = await _invoiceService.ToPrint();
            if (serviceResponse.IsSuccess)
            {
                var response = new ApiResponse<Invoice>
                {
                    Data = serviceResponse.Data,
                    Meta = new { message = serviceResponse.Message }
                };
                return Ok(response);
            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unSuccessfulResponse.Code = "200";
                    unSuccessfulResponse.Message = "No se encontraron Registros";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return Ok(unSuccessfulResponse);
                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado ";
                    unSuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}