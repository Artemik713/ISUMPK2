using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/producttypes")]
    [Authorize]
    public class ProductTypesController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductTypesController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductTypeDto>>> GetAllProductTypes()
        {
            var productTypes = await _productService.GetAllProductTypesAsync();
            return Ok(productTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductTypeDto>> GetProductTypeById(Guid id)
        {
            var productType = await _productService.GetProductTypeByIdAsync(id);
            if (productType == null)
            {
                return NotFound();
            }
            return Ok(productType);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<ProductTypeDto>> CreateProductType([FromBody] ProductTypeCreateDto productTypeDto)
        {
            try
            {
                var createdProductType = await _productService.CreateProductTypeAsync(productTypeDto);
                return CreatedAtAction(nameof(GetProductTypeById), new { id = createdProductType.Id }, createdProductType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<ProductTypeDto>> UpdateProductType(Guid id, [FromBody] ProductTypeCreateDto productTypeDto)
        {
            try
            {
                var updatedProductType = await _productService.UpdateProductTypeAsync(id, productTypeDto);
                if (updatedProductType == null)
                {
                    return NotFound();
                }
                return Ok(updatedProductType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> DeleteProductType(Guid id)
        {
            try
            {
                await _productService.DeleteProductTypeAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
