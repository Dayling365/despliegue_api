using LibreriaDonCesar.API.Models;
using LibreriaDonCesar.API.Services;
using LibreriaDonCesar.Business.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibreriaDonCesar.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest(new
            {
                message = "pageNumber y pageSize deben ser mayores que 0."
            });
        }

        var result = await _logService.GetLogsAsync(pageNumber, pageSize);

        return Ok(new ApiResponse<IEnumerable<SystemLog>>
        {
            Data = result.Data,
            Meta = new
            {
                totalRecords = result.TotalRecords,
                totalPages = result.TotalPages,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            }
        });
    }
}
