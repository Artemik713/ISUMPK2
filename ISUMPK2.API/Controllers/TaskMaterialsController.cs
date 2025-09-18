using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskMaterialsController : ControllerBase
    {
        private readonly ITaskMaterialService _taskMaterialService;

        public TaskMaterialsController(ITaskMaterialService taskMaterialService)
        {
            _taskMaterialService = taskMaterialService;
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetByTaskId(Guid taskId)
        {
            var result = await _taskMaterialService.GetByTaskIdAsync(taskId);
            return Ok(result);
        }

        [HttpGet("material/{materialId}")]
        public async Task<IActionResult> GetByMaterialId(Guid materialId)
        {
            var result = await _taskMaterialService.GetByMaterialIdAsync(materialId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskMaterialCreateDto createDto)
        {
            var result = await _taskMaterialService.CreateTaskMaterialAsync(createDto);
            return CreatedAtAction(nameof(GetByTaskId), new { taskId = result.TaskId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, TaskMaterialUpdateDto updateDto)
        {
            var result = await _taskMaterialService.UpdateTaskMaterialAsync(id, updateDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskMaterialService.DeleteTaskMaterialAsync(id);
            return NoContent();
        }
    }
}