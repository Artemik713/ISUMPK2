using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize] // Любой авторизованный пользователь может получить список для чата
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                Console.WriteLine("=== UsersController.GetAllUsers ===");

                // Получаем текущего пользователя
                var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"Current user ID string: {currentUserIdString}");

                if (!Guid.TryParse(currentUserIdString, out var currentUserId))
                {
                    Console.WriteLine("Failed to parse current user ID");
                    return Unauthorized();
                }

                Console.WriteLine($"Пользователь {currentUserId} запрашивает список всех пользователей для чата");

                // ДЛЯ ЧАТА: Все авторизованные пользователи видят всех других пользователей
                var users = await _userService.GetAllUsersAsync();

                Console.WriteLine($"Возвращаем {users?.Count() ?? 0} пользователей");
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllUsers: {ex.Message}");
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // Добавьте отдельный endpoint для админских функций
        [HttpGet("admin")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersForAdmin()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех пользователей для админа");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // В ISUMPK2.API/Controllers/UsersController.cs
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            // Проверяем id на валидность
            if (id == Guid.Empty)
            {
                return NotFound(new { message = "Указан некорректный ID пользователя" });
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"Пользователь с ID {id} не найден" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении пользователя с ID {id}");
                return StatusCode(500, new { message = "Ошибка сервера при получении данных пользователя" });
            }
        }

        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("by-department/{departmentId}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(Guid departmentId)
        {
            var users = await _userService.GetUsersByDepartmentAsync(departmentId);
            return Ok(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateDto userDto)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(userDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UserUpdateDto userDto)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, userDto);
                return Ok(updatedUser);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize] // Доступен всем авторизованным
        public async Task<ActionResult<UserDto>> GetUserProfile()
        {
            // Используйте безопасную проверку на null
            var userId = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userGuid);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("current")]
        [Authorize] // Доступен всем авторизованным
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                // Добавляем отладочную информацию
                _logger.LogInformation("=== ОТЛАДКА GetCurrentUser ===");
                _logger.LogInformation($"User.Identity.IsAuthenticated: {User.Identity.IsAuthenticated}");
                _logger.LogInformation($"User.Identity.Name: {User.Identity.Name}");

                // Выводим все claims для отладки
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation($"Parsed userId: {userId}");

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с ID {userId} не найден в базе данных");
                    return NotFound("Пользователь не найден");
                }

                _logger.LogInformation($"Найден пользователь: {user.UserName}, роли: {string.Join(", ", user.Roles)}");
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Неавторизованный доступ: {ex.Message}");
                return Unauthorized("Пользователь не авторизован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных текущего пользователя");
                return StatusCode(500, new { message = "Ошибка при получении данных пользователя" });
            }
        }

        private Guid GetCurrentUserId()
        {
            // Пробуем разные варианты получения ID пользователя
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("nameid")?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.Identity.Name;

            _logger.LogInformation($"Попытка парсинга userId из строки: '{userIdString}'");

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogError($"Не удалось спарсить userId из строки: '{userIdString}'");
                throw new UnauthorizedAccessException("Пользователь не авторизован или ID некорректный");
            }
            return userId;
        }
    }
}
