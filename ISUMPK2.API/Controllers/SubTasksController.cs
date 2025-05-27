using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubTasksController : ControllerBase
    {
        private readonly ISubTaskService _subTaskService;
        private readonly ILogger<SubTasksController> _logger;

        public SubTasksController(ISubTaskService subTaskService, ILogger<SubTasksController> logger)
        {
            _subTaskService = subTaskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubTaskDto>>> GetAll()
        {
            var subTasks = await _subTaskService.GetAllSubTasksAsync();
            return Ok(subTasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubTaskDto>> GetById(Guid id)
        {
            var subTask = await _subTaskService.GetSubTaskByIdAsync(id);
            if (subTask == null)
                return NotFound();

            return Ok(subTask);
        }

        [HttpGet("task/{parentTaskId}")]
        public async Task<ActionResult<IEnumerable<SubTaskDto>>> GetByParentTask(Guid parentTaskId)
        {
            try
            {
                var subTasks = await _subTaskService.GetByParentTaskIdAsync(parentTaskId);
                return Ok(subTasks);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                _logger.LogError(ex, "Ошибка при получении подзадач для задачи {TaskId}", parentTaskId);
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }

        [HttpGet("assignee/{assigneeId}")]
        public async Task<ActionResult<IEnumerable<SubTaskDto>>> GetByAssignee(Guid assigneeId)
        {
            var subTasks = await _subTaskService.GetByAssigneeAsync(assigneeId);
            return Ok(subTasks);
        }

        [HttpPost]
        public async Task<ActionResult<SubTaskDto>> Create(SubTaskCreateDto subTaskDto)
        {
            try
            {
                var createdSubTask = await _subTaskService.CreateSubTaskAsync(subTaskDto);
                return CreatedAtAction(nameof(GetById), new { id = createdSubTask.Id }, createdSubTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании подзадачи: {Message}", ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SubTaskDto>> Update(Guid id, SubTaskUpdateDto subTaskDto)
        {
            try
            {
                var updatedSubTask = await _subTaskService.UpdateSubTaskAsync(id, subTaskDto);
                return Ok(updatedSubTask);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _subTaskService.DeleteSubTaskAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}