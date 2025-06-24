using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Application.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Добавить импорт
using ISUMPK2.API.Extensions;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, IUserService userService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _userService = userService;
            _logger = logger;
        }

        // Получить все задачи (доступно администраторам/менеджерам)
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();

            // Обеспечиваем, что все имена исполнителей заполнены
            foreach (var task in tasks)
            {
                if (task.AssigneeId.HasValue && string.IsNullOrEmpty(task.AssigneeName))
                {
                    try
                    {
                        var user = await _userService.GetUserByIdAsync(task.AssigneeId.Value);
                        if (user != null)
                        {
                            task.AssigneeName = $"{user.FirstName} {user.LastName}";
                        }
                    }
                    catch (Exception) { /* Игнорируем ошибки при загрузке пользователя */ }
                }

                // Если имя все равно пустое, но есть ID
                if (task.AssigneeId.HasValue && string.IsNullOrEmpty(task.AssigneeName))
                {
                    task.AssigneeName = $"Пользователь (ID: {task.AssigneeId})";
                }
            }

            return Ok(tasks);
        }


        // Получить задачу по Id
        // ISUMPK2.API/Controllers/TasksController.cs
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskDto>> GetTaskById(Guid id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            // Обеспечиваем, что AssigneeName заполнен всегда
            if (task.AssigneeId.HasValue)
            {
                // Если имени нет, но есть ID - пробуем загрузить имя пользователя
                if (string.IsNullOrEmpty(task.AssigneeName))
                {
                    try
                    {
                        var user = await _userService.GetUserByIdAsync(task.AssigneeId.Value);
                        if (user != null)
                        {
                            task.AssigneeName = $"{user.FirstName} {user.LastName}";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логгируем ошибку, но продолжаем выполнение
                        Console.WriteLine($"Ошибка при загрузке информации о пользователе {task.AssigneeId}: {ex.Message}");
                    }
                }

                // Если имя все равно не загрузилось - используем ID как временное решение
                if (string.IsNullOrEmpty(task.AssigneeName))
                {
                    task.AssigneeName = $"Пользователь (ID: {task.AssigneeId})";
                }
            }
            else
            {
                // Если AssigneeId равен null, явно устанавливаем "Не назначен"
                task.AssigneeName = "Не назначен";
            }

            return Ok(task);
        }

        // Поиск по статусу
        [HttpGet("by-status/{statusId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByStatus(int statusId)
        {
            var tasks = await _taskService.GetTasksByStatusAsync(statusId);
            return Ok(tasks);
        }

        // Поиск по исполнителю
        [HttpGet("by-assignee/{assigneeId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByAssignee(Guid assigneeId)
        {
            var tasks = await _taskService.GetTasksByAssigneeAsync(assigneeId);
            return Ok(tasks);
        }

        // Мои задачи
        [HttpGet("my-tasks")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var tasks = await _taskService.GetTasksByAssigneeAsync(userId);
            return Ok(tasks);
        }

        // Задачи, созданные мной
        [HttpGet("created-by-me")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksCreatedByMe()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var tasks = await _taskService.GetTasksByCreatorAsync(userId);
            return Ok(tasks);
        }

        // Поиск по отделу
        [HttpGet("by-department/{departmentId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByDepartment(Guid departmentId)
        {
            var tasks = await _taskService.GetTasksByDepartmentAsync(departmentId);
            return Ok(tasks);
        }

        // Поиск по приоритету
        [HttpGet("by-priority/{priorityId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByPriority(int priorityId)
        {
            var tasks = await _taskService.GetTasksByPriorityAsync(priorityId);
            return Ok(tasks);
        }

        // Поиск по сроку (диапазон дат)
        [HttpGet("by-due-date")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByDueDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var tasks = await _taskService.GetTasksByDueDateRangeAsync(startDate, endDate);
            return Ok(tasks);
        }

        // Поиск по продукту
        [HttpGet("by-product/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByProduct(Guid productId)
        {
            var tasks = await _taskService.GetTasksByProductAsync(productId);
            return Ok(tasks);
        }

        // Просроченные задачи
        [HttpGet("overdue")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks()
        {
            var tasks = await _taskService.GetOverdueTasksAsync();
            return Ok(tasks);
        }

        // Для дашборда пользователя
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksForDashboard()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var tasks = await _taskService.GetTasksForDashboardAsync(userId);
            return Ok(tasks);
        }

        // СОЗДАНИЕ задачи (POST) — теперь будет отображаться в Swagger!
        [HttpPost]
        [ProducesResponseType(typeof(TaskDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] TaskCreateDto taskDto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            try
            {
                var createdTask = await _taskService.CreateTaskAsync(userId, taskDto);
                // Возвращаем 201 и ссылку на созданную задачу
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Обновление задачи (PUT)
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] TaskUpdateDto taskDto)
        {
            try
            {
                var updatedTask = await _taskService.UpdateTaskAsync(id, taskDto);
                return Ok(updatedTask);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                // Заменяем NotFoundException на проверку KeyNotFoundException или сообщения об ошибке
                return NotFound(new { message = $"Задача с ID {id} не найдена" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // Смена статуса (PATCH)
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(TaskDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] TaskStatusUpdateDto statusDto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            try
            {
                var updatedTask = await _taskService.UpdateTaskStatusAsync(id, userId, statusDto);
                return Ok(updatedTask);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Удаление задачи (DELETE)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector,MetalShopManager,PaintShopManager")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            try
            {
                // Логирование начала операции
                _logger.LogInformation("Attempting to delete task with ID: {TaskId}", id);
                
                // Проверка авторизации пользователя
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Unauthorized access attempt to delete task {TaskId}", id);
                    return Unauthorized();
                }

                // Получение задачи для проверки прав доступа
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found for deletion", id);
                    return NotFound();
                }

                // Проверка прав доступа
                bool isAdmin = User.IsInRole("Administrator");
                bool isManager = User.IsInRole("GeneralDirector") || 
                                 User.IsInRole("MetalShopManager") || 
                                 User.IsInRole("PaintShopManager");
                bool isCreator = task.CreatorId == userId;
                
                _logger.LogInformation("User {UserId} permissions for delete: IsAdmin={IsAdmin}, IsManager={IsManager}, IsCreator={IsCreator}", 
                    userId, isAdmin, isManager, isCreator);
                
                if (!isAdmin && !isManager && !isCreator)
                {
                    _logger.LogWarning("User {UserId} has insufficient permissions to delete task {TaskId}", userId, id);
                    return Forbid();
                }

                // Вызов метода удаления
                await _taskService.DeleteTaskAsync(id);
                
                _logger.LogInformation("Successfully deleted task {TaskId}", id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Application error deleting task {TaskId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // Добавьте этот метод, если его еще нет
        [HttpPost("subtasks")]
        [Authorize(Roles = "Administrator,GeneralDirector,MetalShopManager,PaintShopManager")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskDto>> CreateSubTask([FromBody] TaskCreateDto taskDto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            try
            {
                _logger.LogInformation("Creating subtask by user {UserId}", userId);
                var createdTask = await _taskService.CreateTaskAsync(userId, taskDto);
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Failed to create subtask: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // Добавить комментарий
        [HttpPost("comments")]
        [ProducesResponseType(typeof(TaskCommentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<TaskCommentDto>> AddComment([FromBody] TaskCommentCreateDto commentDto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            try
            {
                var comment = await _taskService.AddCommentAsync(userId, commentDto);
                return Ok(comment);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Получить комментарии задачи
        [HttpGet("{taskId}/comments")]
        [ProducesResponseType(typeof(IEnumerable<TaskCommentDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetCommentsByTaskId(Guid taskId)
        {
            try
            {
                var comments = await _taskService.GetCommentsByTaskIdAsync(taskId);
                return Ok(comments);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}