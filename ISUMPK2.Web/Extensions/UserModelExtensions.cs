using System;
using System.Linq;
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Extensions
{
    /// <summary>
    /// Методы расширения для работы с моделью пользователя
    /// </summary>
    public static class UserModelExtensions
    {
        /// <summary>
        /// Определяет должность пользователя
        /// </summary>
        public static string GetPosition(this UserModel user)
        {
            // По ролям пользователя (если доступны)
            if (user.Roles != null && user.Roles.Any())
            {
                if (user.Roles.Contains("MetalShopManager"))
                    return "Начальник цеха металлообработки";

                if (user.Roles.Contains("PaintShopManager"))
                    return "Начальник цеха металлопокраски";

                if (user.Roles.Contains("Administrator"))
                    return "Администратор";

                if (user.Roles.Contains("Worker"))
                    return "Рабочий цеха";

                return string.Join(", ", user.Roles);
            }

            // По имени пользователя (если роли недоступны)
            string username = user.UserName?.ToLower() ?? "";
            string fullName = user.FullName?.ToLower() ?? "";

            if (username.Contains("admin") || fullName.Contains("админ"))
                return "Администратор";

            if (username.Contains("director") || fullName.Contains("директор"))
                return "Генеральный директор";

            if (username.Contains("manager") || fullName.Contains("начальник"))
            {
                if (username.Contains("metal") || fullName.Contains("металл"))
                    return "Начальник цеха металлообработки";

                if (username.Contains("paint") || fullName.Contains("покрас"))
                    return "Начальник цеха металлопокраски";

                return "Руководитель";
            }

            // По умолчанию
            return "Сотрудник";
        }
    }
}