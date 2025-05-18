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
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
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


        // Добавленный метод для получения текущего пользователя
        [HttpGet("current")]
        [Authorize] // Доступен всем авторизованным
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Пользователь не авторизован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных текущего пользователя");
                return StatusCode(500, new { message = "Ошибка при получении данных пользователя" });
            }
        }

        // Добавленный вспомогательный метод для получения ID текущего пользователя
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован или ID некорректный");
            }
            return userId;
        }
    }
}
