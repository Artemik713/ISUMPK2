using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
namespace ISUMPK2.API.Controllers
{
   
    namespace ISUPMK.API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class TasksController : ControllerBase
        {
            private readonly ITaskService _taskService;

            public TasksController(ITaskService taskService)
            {
                _taskService = taskService;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(tasks);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<TaskDto>> GetTaskById(Guid id)
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound();
                }
                return Ok(task);
            }

            [HttpGet("by-status/{statusId}")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByStatus(int statusId)
            {
                var tasks = await _taskService.GetTasksByStatusAsync(statusId);
                return Ok(tasks);
            }

            [HttpGet("by-assignee/{assigneeId}")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByAssignee(Guid assigneeId)
            {
                var tasks = await _taskService.GetTasksByAssigneeAsync(assigneeId);
                return Ok(tasks);
            }

            [HttpGet("my-tasks")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Unauthorized();
                }

                var tasks = await _taskService.GetTasksByAssigneeAsync(userId);
                return Ok(tasks);
            }

            [HttpGet("created-by-me")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksCreatedByMe()
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Unauthorized();
                }

                var tasks = await _taskService.GetTasksByCreatorAsync(userId);
                return Ok(tasks);
            }

            [HttpGet("by-department/{departmentId}")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByDepartment(Guid departmentId)
            {
                var tasks = await _taskService.GetTasksByDepartmentAsync(departmentId);
                return Ok(tasks);
            }

            [HttpGet("by-priority/{priorityId}")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByPriority(int priorityId)
            {
                var tasks = await _taskService.GetTasksByPriorityAsync(priorityId);
                return Ok(tasks);
            }

            [HttpGet("by-due-date")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByDueDateRange(
                [FromQuery] DateTime startDate,
                [FromQuery] DateTime endDate)
            {
                var tasks = await _taskService.GetTasksByDueDateRangeAsync(startDate, endDate);
                return Ok(tasks);
            }

            [HttpGet("by-product/{productId}")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByProduct(Guid productId)
            {
                var tasks = await _taskService.GetTasksByProductAsync(productId);
                return Ok(tasks);
            }

            [HttpGet("overdue")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks()
            {
                var tasks = await _taskService.GetOverdueTasksAsync();
                return Ok(tasks);
            }

            [HttpGet("dashboard")]
            public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksForDashboard()
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Unauthorized();
                }

                var tasks = await _taskService.GetTasksForDashboardAsync(userId);
                return Ok(tasks);
            }

            [HttpPost]
            public async Task<ActionResult<TaskDto>> CreateTask([FromBody] TaskCreateDto taskDto)
            {
                try
                {
                    if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    {
                        return Unauthorized();
                    }

                    var createdTask = await _taskService.CreateTaskAsync(userId, taskDto);
                    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] TaskUpdateDto taskDto)
            {
                try
                {
                    var updatedTask = await _taskService.UpdateTaskAsync(id, taskDto);
                    return Ok(updatedTask);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpPatch("{id}/status")]
            public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] TaskStatusUpdateDto statusDto)
            {
                try
                {
                    if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    {
                        return Unauthorized();
                    }

                    var updatedTask = await _taskService.UpdateTaskStatusAsync(id, userId, statusDto);
                    return Ok(updatedTask);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpDelete("{id}")]
            [Authorize(Roles = "Administrator,GeneralDirector,MetalShopManager,PaintShopManager")]
            public async Task<ActionResult> DeleteTask(Guid id)
            {
                try
                {
                    await _taskService.DeleteTaskAsync(id);
                    return NoContent();
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpPost("comments")]
            public async Task<ActionResult<TaskCommentDto>> AddComment([FromBody] TaskCommentCreateDto commentDto)
            {
                try
                {
                    if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    {
                        return Unauthorized();
                    }

                    var comment = await _taskService.AddCommentAsync(userId, commentDto);
                    return Ok(comment);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            [HttpGet("{taskId}/comments")]
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

}
