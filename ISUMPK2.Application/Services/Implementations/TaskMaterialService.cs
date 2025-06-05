using ISUMPK2.Application.DTOs;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class TaskMaterialService : ITaskMaterialService
    {
        private readonly ITaskMaterialRepository _taskMaterialRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMaterialRepository _materialRepository;

        public TaskMaterialService(
            ITaskMaterialRepository taskMaterialRepository,
            ITaskRepository taskRepository,
            IMaterialRepository materialRepository)
        {
            _taskMaterialRepository = taskMaterialRepository;
            _taskRepository = taskRepository;
            _materialRepository = materialRepository;
        }

        public async Task<TaskMaterialDto> GetByIdAsync(Guid id)
        {
            var taskMaterial = await _taskMaterialRepository.GetByIdAsync(id);
            if (taskMaterial == null)
                return null;

            return await MapToDto(taskMaterial);
        }

        public async Task<IEnumerable<TaskMaterialDto>> GetByTaskIdAsync(Guid taskId)
        {
            var taskMaterials = await _taskMaterialRepository.GetByTaskIdAsync(taskId);
            var dtoList = new List<TaskMaterialDto>();

            foreach (var taskMaterial in taskMaterials)
            {
                dtoList.Add(await MapToDto(taskMaterial));
            }

            return dtoList;
        }

        public async Task<IEnumerable<TaskMaterialDto>> GetByMaterialIdAsync(Guid materialId)
        {
            var taskMaterials = await _taskMaterialRepository.GetByMaterialIdAsync(materialId);
            var dtoList = new List<TaskMaterialDto>();

            foreach (var taskMaterial in taskMaterials)
            {
                dtoList.Add(await MapToDto(taskMaterial));
            }

            return dtoList;
        }

        public async Task<TaskMaterialDto> CreateTaskMaterialAsync(TaskMaterialCreateDto createDto)
        {
            // Проверка существования задачи
            var task = await _taskRepository.GetByIdAsync(createDto.TaskId);
            if (task == null)
                throw new InvalidOperationException($"Задача с ID {createDto.TaskId} не найдена");

            // Проверка существования материала
            var material = await _materialRepository.GetByIdAsync(createDto.MaterialId);
            if (material == null)
                throw new InvalidOperationException($"Материал с ID {createDto.MaterialId} не найден");

            var taskMaterial = new TaskMaterial
            {
                TaskId = createDto.TaskId,
                MaterialId = createDto.MaterialId,
                Quantity = createDto.Quantity
            };

            await _taskMaterialRepository.AddAsync(taskMaterial);
            await _taskMaterialRepository.SaveChangesAsync();

            return await GetByIdAsync(taskMaterial.Id);
        }

        public async Task<TaskMaterialDto> UpdateTaskMaterialAsync(Guid id, TaskMaterialUpdateDto updateDto)
        {
            var taskMaterial = await _taskMaterialRepository.GetByIdAsync(id);
            if (taskMaterial == null)
                throw new InvalidOperationException($"Связь задачи и материала с ID {id} не найдена");

            taskMaterial.Quantity = updateDto.Quantity;

            await _taskMaterialRepository.UpdateAsync(taskMaterial);
            await _taskMaterialRepository.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task DeleteTaskMaterialAsync(Guid id)
        {
            await _taskMaterialRepository.DeleteAsync(id);
            await _taskMaterialRepository.SaveChangesAsync();
        }

        private async Task<TaskMaterialDto> MapToDto(TaskMaterial taskMaterial)
        {
            var material = taskMaterial.Material ??
                await _materialRepository.GetByIdAsync(taskMaterial.MaterialId);

            return new TaskMaterialDto
            {
                Id = taskMaterial.Id,
                TaskId = taskMaterial.TaskId,
                MaterialId = taskMaterial.MaterialId,
                MaterialName = material?.Name,
                MaterialCode = material?.Code,
                UnitOfMeasure = material?.UnitOfMeasure,
                Quantity = taskMaterial.Quantity
            };
        }
    }
}