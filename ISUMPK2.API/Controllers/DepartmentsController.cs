using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAllDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartmentById(Guid id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] DepartmentCreateDto departmentDto)
        {
            try
            {
                var createdDepartment = await _departmentService.CreateDepartmentAsync(departmentDto);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = createdDepartment.Id }, createdDepartment);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<DepartmentDto>> UpdateDepartment(Guid id, [FromBody] DepartmentUpdateDto departmentDto)
        {
            try
            {
                var updatedDepartment = await _departmentService.UpdateDepartmentAsync(id, departmentDto);
                if (updatedDepartment == null)
                {
                    return NotFound();
                }
                return Ok(updatedDepartment);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> DeleteDepartment(Guid id)
        {
            try
            {
                await _departmentService.DeleteDepartmentAsync(id);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
