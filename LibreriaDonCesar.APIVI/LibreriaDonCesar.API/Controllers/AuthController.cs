using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.Business.Services;
using LibreriaDonCesar.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibreriaDonCesar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


       [HttpGet("byname/{name}")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

            var serviceResponse = await _authService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess)
            {
                var userDto = new UserDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data.UserName,
                    Email= serviceResponse.Data.Email
                };

                return Ok(userDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró el usuario al valor de Name proporcionado";
                    unSuccessfulResponse.Details = serviceResponse.Details = new { Error = "No hay registros asociados al valor del Name proporcionado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unSuccessfulResponse);
            }
        }



        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto newUser)
        {
            var serviceResponse = await _authService.RegisterAsync(newUser);

            if (serviceResponse.IsSuccess)
            {
                var newUserDto = new AuthDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data!.UserName,
                    Email = serviceResponse.Data.Email,
                    

                    //State = serviceResponse.Data!.State
                };

                return CreatedAtAction(
                    nameof(GetByName),
                    new { name = newUserDto.UserName },
                    newUserDto
                );
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre del usuario o correo ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar un nombre o correo del usuario" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno inesperado" };

                    return BadRequest(unSuccessfulResponse);
            }
        }




       [HttpGet("byemail/{email}")]
        private async Task<IActionResult> GetByEmail(string email)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            if (string.IsNullOrWhiteSpace(email))
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                unSuccessfulResponse.Details = new { Error = "El Email no puede ser nulo o vacío" };
                return BadRequest(unSuccessfulResponse);
            }

            var serviceResponse = await _authService.GetByEmailAsync(email);

            if (serviceResponse.IsSuccess)
            {
                var userDto = new UserDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data.UserName,
                    Email = serviceResponse.Data.Email
                };

                return Ok(userDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró el usuario al valor de Email proporcionado";
                    unSuccessfulResponse.Details = serviceResponse.Details = new { Error = "No hay registros asociados al valor del Email proporcionado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            //Confiamos las validaciones del Dto al Framework

            var serviceResponse = await _authService.LoginAsync(loginRequest);

            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Unauthorized:
                    unSuccessfulResponse.Code = "401";
                    unSuccessfulResponse.Message = "Error de autenticacion de usuario";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };


                    return Unauthorized(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }



    }
}
