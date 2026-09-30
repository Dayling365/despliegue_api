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
    public class PresentationsController : ControllerBase
    {
        private readonly IPresentationService _presentationService;

        public PresentationsController(IPresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _presentationService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var presentationsDtoCollection = serviceResponse.Data.Data.Select(p => new PresentationDto
                {
                    Id = p.Id,
                    PresentationName = p.PresentationName,
                    Amount = p.Amount,
                    UnitMeasureId =p.UnitMeasureId,
                    UnitMeasureName = p.UnitMeasureName,
                });

                var apiResponse = new ApiResponse<IEnumerable<PresentationDto>>
                {
                    Data = presentationsDtoCollection,
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
        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            //validar que el id cumple con el formato esperado
            if (id<=0 || id==null)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }
            var serviceResponse = await _presentationService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {

                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    Amount = serviceResponse.Data!.Amount,
                    UnitMeasureId = serviceResponse.Data!.UnitMeasureId,
                    UnitMeasureName = serviceResponse.Data!.UnitMeasureName,
                };

                return Ok(presentationDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "404",
                        Message = "No se encontro una presentation asociada al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };

                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse = new UnsuccessfulResponseDto
                    {
                        Code = "500",
                        Message =  "Ocurrió un error",
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

            var serviceResponse = await _presentationService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    PresentationName = serviceResponse.Data.PresentationName,
                    Amount = serviceResponse.Data.Amount,
                    UnitMeasureId = serviceResponse.Data.UnitMeasureId,
                    UnitMeasureName = serviceResponse.Data.UnitMeasureName,
                };

                return Ok(presentationDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró la presentacion asociado al valor de Name proporcionado";
                    unSuccessfulResponse.Details = serviceResponse.Details = new { Error = "No hay registros asociados al valor de Name proporcionado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreatePresentationDto presentationDto)
        {
            var serviceResponse = await _presentationService.CreateAsync(presentationDto);

            if (serviceResponse.IsSuccess)
            {
                var newPresentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    Amount = serviceResponse.Data!.Amount,
                    UnitMeasureId = serviceResponse.Data!.UnitMeasureId,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newPresentationDto.Id },
                    newPresentationDto
                );
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la presentacion ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar un nombre de la presentacion" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno inesperado" };

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]

        public async Task<IActionResult> Update(int id, [FromBody] UpdatePresentationDto presentationDto)
        {

            var serviceResponse = await _presentationService.UpdateAsync(id, presentationDto);
            if (serviceResponse.IsSuccess)
            {
                var updatedPresentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data!.Id,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    Amount = serviceResponse.Data!.Amount,
                    UnitMeasureId = serviceResponse.Data!.UnitMeasureId,
                    
                };
                return Ok(updatedPresentationDto);
            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontro presentacion con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto con el nombre de la presentacion, no debe duplicarse" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }

        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _presentationService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    Id = serviceResponse.Data!.Id,
                    PresentationName = serviceResponse.Data.PresentationName,
                    State = serviceResponse.Data.State,
                    Message = serviceResponse.Message
                });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto
            {
                Code = serviceResponse.MessageCode == MessageCodes.ErrorValidation ? "400" : "500",
                Message = serviceResponse.Message,
                Details = new { info = "Error al cambiar el estado" }
            };

            return BadRequest(unSuccessfulResponse);
        }
    }
}
