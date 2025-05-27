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
using ISUMPK2.Application.Services.Implementations;

namespace ISUMPK2.API.Controllers
{
    // ISUMPK2.API/Controllers/MaterialsController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialsController> _logger;

        public MaterialsController(IMaterialService materialService, ILogger<MaterialsController> logger)
        {
            _materialService = materialService;
            _logger = logger;
        }

        // Добавляем методы для работы с категориями

        [HttpGet("categories")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialCategoryDto>>> GetAllCategories()
        {
            var categories = await _materialService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // Убедитесь, что этот метод существует в API контроллере
        [HttpPost("{materialId}/transactions")]
        [Authorize]
        public async Task<IActionResult> AddMaterialTransaction(Guid materialId, [FromBody] MaterialTransactionCreateDto transactionDto)
        {
            if (materialId != transactionDto.MaterialId)
            {
                return BadRequest("ID материала в пути и в модели данных не совпадают");
            }

            try
            {
                // Устанавливаем значение по умолчанию для Notes
                if (string.IsNullOrEmpty(transactionDto.Notes))
                {
                    transactionDto.Notes = "-";
                }

                // Получаем ID пользователя из токена
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Если найти не удалось, возвращаем явную ошибку для диагностики
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token. Claims: {Claims}",
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
                    return Unauthorized("ID пользователя не найден в токене авторизации");
                }

                if (!Guid.TryParse(userId, out var userGuid))
                {
                    _logger.LogWarning("Cannot parse user ID '{UserId}' as GUID", userId);
                    return BadRequest("ID пользователя не является действительным GUID");
                }

                _logger.LogInformation("Adding material transaction for material {MaterialId} by user {UserId}",
                    materialId, userGuid);

                var result = await _materialService.AddTransactionAsync(userGuid, transactionDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении транзакции материала: {Message}", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("categories/top-level")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialCategoryDto>>> GetTopLevelCategories()
        {
            var categories = await _materialService.GetTopLevelCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("categories/{id}")]
        [Authorize]
        public async Task<ActionResult<MaterialCategoryDto>> GetCategoryById(Guid id)
        {
            var category = await _materialService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet("categories/{id}/subcategories")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialCategoryDto>>> GetSubcategories(Guid id)
        {
            var categories = await _materialService.GetSubcategoriesAsync(id);
            return Ok(categories);
        }

        [HttpGet("categories/{id}/materials")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsByCategory(
            Guid id, [FromQuery] bool includeSubcategories = false)
        {
            var materials = await _materialService.GetMaterialsByCategoryAsync(id, includeSubcategories);
            return Ok(materials);
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialCategoryDto>> CreateCategory([FromBody] MaterialCategoryDto categoryDto)
        {
            var createdCategory = await _materialService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] MaterialCreateDto materialDto)
        {
            try
            {
                var createdMaterial = await _materialService.CreateMaterialAsync(materialDto);
                return CreatedAtAction(nameof(GetMaterialById), new { id = createdMaterial.Id }, createdMaterial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material");
                return StatusCode(500, "Внутренняя ошибка при создании материала");
            }
        }
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialCategoryDto>> UpdateCategory(Guid id, [FromBody] MaterialCategoryDto categoryDto)
        {
            var updatedCategory = await _materialService.UpdateCategoryAsync(id, categoryDto);
            if (updatedCategory == null)
                return NotFound();

            return Ok(updatedCategory);
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            await _materialService.DeleteCategoryAsync(id);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector,Storekeeper")]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(Guid id, [FromBody] MaterialUpdateDto materialDto)
        {
            try
            {
                var updatedMaterial = await _materialService.UpdateMaterialAsync(id, materialDto);
                if (updatedMaterial == null)
                    return NotFound();

                return Ok(updatedMaterial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material");
                return StatusCode(500, "Внутренняя ошибка при обновлении материала");
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> SearchMaterials([FromQuery] string searchTerm)
        {
            var materials = await _materialService.SearchMaterialsAsync(searchTerm);
            return Ok(materials);
        }

        // Дополним контроллер базовыми методами для работы с материалами

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetAllMaterials()
        {
            var materials = await _materialService.GetAllMaterialsAsync();
            return Ok(materials);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MaterialDto>> GetMaterialById(Guid id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null)
                return NotFound();

            return Ok(material);
        }

        [HttpGet("low-stock")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsWithLowStock()
        {
            var materials = await _materialService.GetMaterialsWithLowStockAsync();
            return Ok(materials);
        }
    }
}
