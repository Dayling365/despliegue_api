using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Business.DTOs;
using LibreriaDonCesar.core.Common;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;

namespace LibreriaDonCesar.Business.Interfaces
{
    public interface IAuthService
    {

        Task<ServiceResponse<User>> RegisterAsync(RegisterUserDto newUser);

        Task<ServiceResponse<User>> GetByNameAsync(string name);

        Task<ServiceResponse<User>> GetByEmailAsync(string email);


       Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);




    }
}
