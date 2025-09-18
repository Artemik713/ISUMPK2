using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;

namespace ISUMPK2.Web.Extensions
{
    /// <summary>
    /// Методы расширения для работы с задачами отделов
    /// </summary>
    public static class TaskServiceExtensions
    {
        /// <summary>
        /// Получает задачи для конкретного отдела
        /// </summary>
        public static async Task<IEnumerable<TaskDto>> GetTasksAsync(this ITaskService taskService, Guid departmentId)
        {
            try
            {
                // Загружаем все задачи и фильтруем на клиенте
                var allTasks = await taskService.GetAllTasksAsync();

                // Фильтруем задачи по ID отдела
                return allTasks
                    .Where(t => t.DepartmentId.HasValue && t.DepartmentId.Value == departmentId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке задач отдела: {ex.Message}");
                return new List<TaskDto>();
            }
        }
    }
}