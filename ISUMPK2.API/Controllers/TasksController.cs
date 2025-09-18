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
        private readonly IMaterialService _materialService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, IUserService userService, ILogger<TasksController> logger, IMaterialService materialService)
        {
            _taskService = taskService;
            _userService = userService;
            _logger = logger;
            _materialService = materialService;
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

        [HttpDelete("{id}/force")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForceDeleteTask(Guid id)
        {
            try
            {
                _logger.LogInformation("Force deletion requested for task {TaskId}", id);

                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                // Только администраторы и генеральный директор могут принудительно удалять
                bool isAdmin = User.IsInRole("Administrator");
                bool isGeneralDirector = User.IsInRole("GeneralDirector");

                if (!isAdmin && !isGeneralDirector)
                {
                    _logger.LogWarning("User {UserId} attempted force delete without sufficient permissions", userId);
                    return Forbid("Принудительное удаление доступно только администраторам"); // ИСПРАВЛЕНО: используем строку
                }

                await _taskService.ForceDeleteTaskAsync(id);

                _logger.LogInformation("Successfully force deleted task {TaskId}", id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Application error force deleting task {TaskId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message); // ИСПРАВЛЕНО: используем строку напрямую
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force deleting task {TaskId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера"); // ИСПРАВЛЕНО: используем строку
            }
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

        [HttpPut("{id}/materials")]
        public async Task<IActionResult> UpdateTaskMaterials(Guid id, [FromBody] List<TaskMaterialCreateDto> materials)
        {
            try
            {
                _logger.LogInformation($"Получен запрос на обновление материалов для задачи {id}, количество материалов: {materials?.Count ?? 0}");

                if (materials == null)
                {
                    return BadRequest("Список материалов не может быть пустым");
                }

                // Сначала обновляем список материалов задачи
                await _taskService.UpdateTaskMaterialsAsync(id, materials);

                // Затем резервируем материалы (уменьшаем запас)
                await _taskService.ReserveMaterialsAsync(id, materials);

                return Ok(new { message = "Материалы успешно обновлены и зарезервированы" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении материалов задачи {id}: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Внутренняя ошибка сервера при обновлении материалов задачи {id}");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
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

        [HttpPost("{id}/reserve-materials")]
        public async Task<IActionResult> ReserveMaterials(Guid id, [FromBody] List<TaskMaterialCreateDto> materials)
        {
            try
            {
                if (materials == null || !materials.Any())
                {
                    return BadRequest("Не указаны материалы для резервирования");
                }

                // Получаем задачу для проверки её существования
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound($"Задача с ID {id} не найдена");
                }

                // Резервируем материалы для задачи
                foreach (var material in materials)
                {
                    // Получаем данные о материале
                    var materialItem = await _materialService.GetMaterialByIdAsync(material.MaterialId);
                    if (materialItem == null)
                    {
                        return NotFound($"Материал с ID {material.MaterialId} не найден");
                    }

                    if (materialItem.CurrentStock < material.Quantity)
                    {
                        return BadRequest($"Недостаточно материала {materialItem.Name} на складе. Доступно: {materialItem.CurrentStock} {materialItem.UnitOfMeasure}");
                    }

                    // Создаем транзакцию расхода материала
                    var transaction = new MaterialTransactionCreateDto
                    {
                        MaterialId = material.MaterialId,
                        Quantity = material.Quantity,
                        TransactionType = "Issue", // Расход
                        TaskId = id,
                        Notes = $"Зарезервировано для задачи #{task.Title}"
                    };

                    await _materialService.AddMaterialTransactionAsync(transaction);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при резервировании материалов для задачи {id}");
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
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
        [HttpGet("{id}/dependencies")]
        [ProducesResponseType(typeof(TaskDependencyInfoDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TaskDependencyInfoDto>> GetTaskDependencies(Guid id)
        {
            try
            {
                var dependencies = await _taskService.GetTaskDependenciesAsync(id);
                return Ok(dependencies);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = $"Задача с ID {id} не найдена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies for task {TaskId}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
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