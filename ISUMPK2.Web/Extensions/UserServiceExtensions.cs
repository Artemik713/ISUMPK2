using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;

namespace ISUMPK2.Web.Extensions
{
    /// <summary>
    /// Методы расширения для работы с пользователями отделов
    /// </summary>
    public static class UserServiceExtensions
    {
        /// <summary>
        /// Получает пользователей, связанных с указанным отделом
        /// </summary>
        public static async Task<IEnumerable<UserDto>> GetUsersByFilterAsync(this IUserService userService, string filter)
        {
            try
            {
                // Получаем всех пользователей
                var allUsers = await userService.GetAllUsersAsync();

                // Если есть фильтр по отделу
                if (!string.IsNullOrEmpty(filter) && filter.Contains("departmentId eq"))
                {
                    string departmentIdStr = filter.Split(' ').Last();
                    if (Guid.TryParse(departmentIdStr, out Guid departmentId))
                    {
                        // Фильтруем пользователей по отделу
                        return allUsers
                            .Where(u => u.DepartmentId.HasValue && u.DepartmentId.Value == departmentId)
                            .ToList();
                    }
                }

                return allUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке пользователей: {ex.Message}");
                return new List<UserDto>();
            }
        }
    }
}