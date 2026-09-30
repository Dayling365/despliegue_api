using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.Business.Interfaces;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using LibreriaDonCesar.DataAccess.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LibreriaDonCesar.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        public AuthService(IAuthRepository authRepository ,IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;

        }

        private string GenerateTokenJWT(User user, IEnumerable<string>roles)
        {
            var secretkey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];


            var claims = new List<Claim>
            { 
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name,user.UserName),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())

            };

            foreach (var role in roles)
            {
                claims.Add(new Claim (ClaimTypes.Role,role));
            }

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var credencials = new SigningCredentials(Key,SecurityAlgorithms.HmacSha256);

            var exprires = DateTime.UtcNow.AddHours(3);

            var token = new JwtSecurityToken(
                 issuer: issuer,
                audience: audience,
                claims: claims,
                signingCredentials: credencials,
                expires: exprires);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<User>> RegisterAsync(RegisterUserDto newUser)
        {
            try
            {

                var existingUser = await _authRepository.GetByNameAsync(newUser.UserName);

                if (existingUser.OperationStatusCode == 0 && existingUser.Data != null && !string.IsNullOrEmpty(existingUser.Data.UserName))
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario con el nombre proporcionado"
                    };
                }



                var userEntity = new User
                {
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    State = true
                };


                var repoResponse = await _authRepository.RegisterAsync(userEntity);


                switch (repoResponse.OperationStatusCode)
                {
                    case 0:
                        return new ServiceResponse<User>
                        {
                            Data = repoResponse.Data,
                            IsSuccess = true,
                            MessageCode = MessageCodes.Success,
                            Message = "Usuario registrado correctamente."
                        };

                    case 50020:
                        return new ServiceResponse<User>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "El usuario o correo  ya existe."
                        };

                    default:
                        return new ServiceResponse<User>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = "Ocurrió un error inesperado al registrar el usuario."
                        };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }


        public async Task<ServiceResponse<User>> GetByNameAsync(string name)
        {
            var result = await _authRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<User>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 50042:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontro el usuario con ese Name proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el usuario.";
                    break;
            }

            // Retorno final para los casos de error o no encontrado ////
            return new ServiceResponse<User>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }

        public async Task<ServiceResponse<User>> GetByEmailAsync(string email)
        {
            var result = await _authRepository.GetByEmailAsync(email);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<User>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;


            switch (result.OperationStatusCode)
            {
                case 50022:
                    messageCode = MessageCodes.NotFound;
                    message = "No existe el Email";
                    break;

                default:

                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el Email.";
                    break;


            }

            return new ServiceResponse<User>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var existentUser = await _authRepository.GetByNameAsync(loginRequest.UserName);

                if (existentUser.Data!.Id == 0 && existentUser.Data!.UserName.IsNullOrEmpty())
                {

                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = "No existe usuario registrado con el name proporcionado"
                    };


                }


                var isValidPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, existentUser.Data!.PasswordHash);

                if (!isValidPassword)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = " El password no coincide con el registrado"
                    };

                }

                var roles = await _authRepository.GetRolesByUserIdAsync(existentUser.Data.Id);



                var token = GenerateTokenJWT(existentUser.Data!, roles.Data!);

                //Mapeo  de los datos que se enviaran en  la respuesta
                var loginResponse = new LoginResponseDto
                {
                    Id = existentUser.Data!.Id,
                    UserName = existentUser.Data!.UserName,
                    Email = existentUser.Data!.Email,
                    Token = token,
                    Roles = roles.Data!
                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Data = loginResponse,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Login correcto"
                };
            }

            catch (Exception)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio algo inesperado"
                };
            }



        }


        
    }

}  





    

