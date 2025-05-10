using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ISUMPK2.API.Controllers
{
    [Route("api/materials")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialsController> _logger;

        public MaterialsController(IMaterialService materialService, ILogger<MaterialsController> logger)
        {
            _materialService = materialService;
            _logger = logger;
        }

        // GET: api/materials
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAllMaterials()
        {
            try
            {
                var materials = await _materialService.GetAllMaterialsAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all materials");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving materials");
            }
        }

        // GET: api/materials/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MaterialDto>> GetMaterial(Guid id)
        {
            try
            {
                var material = await _materialService.GetMaterialByIdAsync(id);
                if (material == null)
                {
                    return NotFound();
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving material");
            }
        }

        // GET: api/materials/lowstock
        [HttpGet("lowstock")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsWithLowStock()
        {
            try
            {
                var materials = await _materialService.GetMaterialsWithLowStockAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting materials with low stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving materials with low stock");
            }
        }

        // POST: api/materials
        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialDto>> CreateMaterial(MaterialCreateDto materialCreateDto)
        {
            try
            {
                var material = await _materialService.CreateMaterialAsync(materialCreateDto);
                return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating material");
            }
        }

        // PUT: api/materials/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<IActionResult> UpdateMaterial(Guid id, MaterialUpdateDto materialUpdateDto)
        {
            try
            {
                var updatedMaterial = await _materialService.UpdateMaterialAsync(id, materialUpdateDto);
                if (updatedMaterial == null)
                {
                    return NotFound();
                }
                return Ok(updatedMaterial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating material");
            }
        }

        // DELETE: api/materials/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<IActionResult> DeleteMaterial(Guid id)
        {
            try
            {
                await _materialService.DeleteMaterialAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting material");
            }
        }

        // GET: api/materials/5/transactions
        [HttpGet("{id}/transactions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialTransactionDto>>> GetMaterialTransactions(Guid id)
        {
            try
            {
                var transactions = await _materialService.GetTransactionsByMaterialAsync(id);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving transactions");
            }
        }

        // POST: api/materials/transactions
        [HttpPost("transactions")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialTransactionDto>> AddTransaction(MaterialTransactionCreateDto transactionDto)
        {
            try
            {
                // Получаем идентификатор текущего пользователя из клеймов
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                {
                    return BadRequest("Invalid user ID");
                }

                var transaction = await _materialService.AddTransactionAsync(userGuid, transactionDto);
                return Ok(transaction);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error during material transaction");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding material transaction");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding transaction");
            }
        }

        // GET: api/materials/5/check-stock/10
        [HttpGet("{id}/check-stock/{quantity}")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckStock(Guid id, decimal quantity)
        {
            try
            {
                var hasStock = await _materialService.HasSufficientStockAsync(id, quantity);
                return Ok(hasStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error checking stock");
            }
        }

        // PUT: api/materials/5/stock
        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromQuery] decimal quantity, [FromQuery] bool isAddition)
        {
            try
            {
                // Получаем текущий материал
                var material = await _materialService.GetMaterialByIdAsync(id);
                if (material == null)
                {
                    return NotFound();
                }

                // Обновляем запас материала
                decimal newStock = material.CurrentStock;
                if (isAddition)
                {
                    newStock += quantity;
                }
                else
                {
                    newStock -= quantity;
                    // Проверка на отрицательный запас
                    if (newStock < 0)
                    {
                        return BadRequest("Недостаточно запаса материала для списания");
                    }
                }

                // Создаем объект для обновления с новым запасом
                var updateDto = new MaterialUpdateDto
                {
                    Code = material.Code,
                    Name = material.Name,
                    Description = material.Description,
                    UnitOfMeasure = material.UnitOfMeasure,
                    MinimumStock = material.MinimumStock,
                    Price = material.Price,
                    CurrentStock = newStock // Добавляем CurrentStock в DTO для обновления
                };

                // Обновляем материал
                var updatedMaterial = await _materialService.UpdateMaterialAsync(id, updateDto);
                return Ok(updatedMaterial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for material with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating stock");
            }
        }
    }
}
