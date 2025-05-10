using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByType(Guid typeId)
        {
            var products = await _productService.GetProductsByTypeAsync(typeId);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            try
            {
                var createdProduct = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] ProductUpdateDto productDto)
        {
            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, productDto);
                if (updatedProduct == null)
                {
                    return NotFound();
                }
                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/materials")]
        public async Task<ActionResult<IEnumerable<ProductMaterialDto>>> GetProductMaterials(Guid id)
        {
            var materials = await _productService.GetProductMaterialsAsync(id);
            return Ok(materials);
        }

        [HttpGet("{id}/transactions")]
        public async Task<ActionResult<IEnumerable<ProductTransactionDto>>> GetTransactionsByProduct(Guid id)
        {
            var transactions = await _productService.GetTransactionsByProductAsync(id);
            return Ok(transactions);
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<ProductTransactionDto>>> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var transactions = await _productService.GetTransactionsByDateRangeAsync(startDate, endDate);
            return Ok(transactions);
        }

        [HttpPost("transactions")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<ProductTransactionDto>> AddTransaction([FromBody] ProductTransactionCreateDto transactionDto)
        {
            try
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Unauthorized();
                }

                var transaction = await _productService.AddTransactionAsync(userId, transactionDto);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/check-materials/{quantity}")]
        public async Task<ActionResult<bool>> CheckSufficientMaterials(Guid id, decimal quantity)
        {
            var hasSufficientMaterials = await _productService.HasSufficientMaterialsForProductionAsync(id, quantity);
            return Ok(hasSufficientMaterials);
        }

        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult> UpdateStock(Guid id, [FromQuery] decimal quantity, [FromQuery] bool isAddition)
        {
            try
            {
                // В реальной реализации здесь должен быть вызов соответствующего метода сервиса
                // await _productService.UpdateStockAsync(id, quantity, isAddition);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
