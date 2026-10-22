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
    /// <summary>Variantes vendibles (producto base + color + presentación + precio de venta). Primero se crea el producto base en /api/BaseProducts y luego sus variantes aquí.</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var serviceResponse = await _productService.GetAllAsync(pagination);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var items = serviceResponse.Data.Data!.Select(x => new ProductDto
                {
                    Id = x.Id,
                    BaseProductId = x.BaseProductId,
                    ProductName = x.ProductName,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    BrandId = x.BrandId,
                    BrandName = x.BrandName,
                    ColorId = x.ColorId,
                    ColorName = x.ColorName,
                    PresentationId = x.PresentationId,
                    PresentationName = x.PresentationName,
                    SalePrice = x.SalePrice,
                    State = x.State,
                }).ToList();

                var apiResponse = new ApiResponse<IEnumerable<ProductDto>>
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
            //validar que el id cumpla con el formato esperado
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

            var serviceResponse = await _productService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                //Mapeo del producto recibido
                var productDto = new ProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    BaseProductId = serviceResponse.Data!.BaseProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    ColorId = serviceResponse.Data!.ColorId,
                    ColorName = serviceResponse.Data!.ColorName,
                    PresentationId = serviceResponse.Data!.PresentationId,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    State = serviceResponse.Data!.State,
                };

                return Ok(productDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontro  producto asociado al Id proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        /// <summary>Devuelve todas las variantes (color / presentación) del producto con ese nombre.</summary>
        [Authorize(Roles = "Vendedor, Administrador")]
        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var badRequestResponse = new UnsuccessfulResponseDto();
                badRequestResponse.Code = "400";
                badRequestResponse.Message = "El dato proporcionado no es valido";
                badRequestResponse.Details = new { Error = "El name no puede ser nulo o vacio" };

                return BadRequest(badRequestResponse);
            }

            var serviceResponse = await _productService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productsDto = serviceResponse.Data.Select(p => new ProductDto
                {
                    Id = p.Id,
                    BaseProductId = p.BaseProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.CategoryName,
                    BrandId = p.BrandId,
                    BrandName = p.BrandName,
                    ColorId = p.ColorId,
                    ColorName = p.ColorName,
                    PresentationId = p.PresentationId,
                    PresentationName = p.PresentationName,
                    SalePrice = p.SalePrice,
                    State = p.State,
                }).ToList();

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Data = productsDto,
                    Meta = new { message = serviceResponse.Message }
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No hay registro asociado al valor name proporcionado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateProductDto productDto)
        {
            var serviceResponse = await _productService.CreateAsync(productDto);

            if (serviceResponse.IsSuccess)
            {
                var newProductDto = new ProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    BaseProductId = serviceResponse.Data!.BaseProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    ColorId = serviceResponse.Data!.ColorId,
                    ColorName = serviceResponse.Data!.ColorName,
                    PresentationId = serviceResponse.Data!.PresentationId,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    State = serviceResponse.Data!.State,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newProductDto.Id },
                    newProductDto
                    );
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo un conflicto" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dataProduct)
        {
            var serviceResponse = await _productService.UpdateAsync(id, dataProduct);

            if (serviceResponse.IsSuccess)
            {
                var updatedProduct = new ProductDto
                {
                    Id = serviceResponse.Data!.Id,
                    BaseProductId = serviceResponse.Data!.BaseProductId,
                    ProductName = serviceResponse.Data!.ProductName,
                    Description = serviceResponse.Data!.Description,
                    CategoryId = serviceResponse.Data!.CategoryId,
                    CategoryName = serviceResponse.Data!.CategoryName,
                    BrandId = serviceResponse.Data!.BrandId,
                    BrandName = serviceResponse.Data!.BrandName,
                    ColorId = serviceResponse.Data!.ColorId,
                    ColorName = serviceResponse.Data!.ColorName,
                    PresentationId = serviceResponse.Data!.PresentationId,
                    PresentationName = serviceResponse.Data!.PresentationName,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    State = serviceResponse.Data!.State,
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedProduct);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Conflict:
                    unsuccessfulResponse.Code = "409";
                    unsuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo un conflicto" };
                    return Conflict(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _productService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                return Ok(new
                {
                    Id = serviceResponse.Data!.Id,
                    BaseProductId = serviceResponse.Data!.BaseProductId,
                    ColorId = serviceResponse.Data!.ColorId,
                    PresentationId = serviceResponse.Data!.PresentationId,
                    SalePrice = serviceResponse.Data!.SalePrice,
                    State = serviceResponse.Data!.State,
                    Message = serviceResponse.Message
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró el recurso solicitado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unsuccessfulResponse.Code = "400";
                    unsuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicacion" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }
    }
}
