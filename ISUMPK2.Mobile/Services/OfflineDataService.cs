using ISUMPK2.Mobile.Models;
using Microsoft.Maui.Storage;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Services
{
    public interface IOfflineDataService
    {
        Task InitializeAsync();

        // Методы для работы с задачами
        Task<List<TaskModel>> GetTasksAsync();
        Task<TaskModel> GetTaskAsync(Guid id);
        Task SaveTaskAsync(TaskModel task);
        Task SaveTasksAsync(List<TaskModel> tasks);

        // Методы для работы с уведомлениями
        Task<List<NotificationModel>> GetNotificationsAsync();
        Task<NotificationModel> GetNotificationAsync(Guid id);
        Task SaveNotificationAsync(NotificationModel notification);
        Task SaveNotificationsAsync(List<NotificationModel> notifications);
        Task MarkNotificationAsReadAsync(Guid id);
        Task MarkAllNotificationsAsReadAsync();
    }

    public class OfflineDataService : IOfflineDataService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "isupmk.db");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<TaskModel>();
            await _database.CreateTableAsync<TaskCommentModel>();
            await _database.CreateTableAsync<NotificationModel>();

            _isInitialized = true;
        }

        // Методы для работы с задачами
        public async Task<List<TaskModel>> GetTasksAsync()
        {
            await InitializeAsync();
            var tasks = await _database.Table<TaskModel>().ToListAsync();

            // Загружаем комментарии для каждой задачи
            foreach (var task in tasks)
            {
                task.Comments = await _database.Table<TaskCommentModel>()
                    .Where(c => c.TaskId == task.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }

            return tasks;
        }

        public async Task<TaskModel> GetTaskAsync(Guid id)
        {
            await InitializeAsync();
            var task = await _database.Table<TaskModel>().Where(t => t.Id == id).FirstOrDefaultAsync();

            if (task != null)
            {
                task.Comments = await _database.Table<TaskCommentModel>()
                    .Where(c => c.TaskId == id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }

            return task;
        }

        public async Task SaveTaskAsync(TaskModel task)
        {
            await InitializeAsync();

            // Сохраняем задачу
            var existingTask = await _database.Table<TaskModel>().Where(t => t.Id == task.Id).FirstOrDefaultAsync();
            if (existingTask != null)
            {
                await _database.UpdateAsync(task);
            }
            else
            {
                await _database.InsertAsync(task);
            }

            // Сохраняем комментарии
            if (task.Comments != null)
            {
                foreach (var comment in task.Comments)
                {
                    var existingComment = await _database.Table<TaskCommentModel>()
                        .Where(c => c.Id == comment.Id)
                        .FirstOrDefaultAsync();

                    if (existingComment != null)
                    {
                        await _database.UpdateAsync(comment);
                    }
                    else
                    {
                        await _database.InsertAsync(comment);
                    }
                }
            }
        }

        public async Task SaveTasksAsync(List<TaskModel> tasks)
        {
            await InitializeAsync();

            foreach (var task in tasks)
            {
                await SaveTaskAsync(task);
            }
        }

        // Методы для работы с уведомлениями
        public async Task<List<NotificationModel>> GetNotificationsAsync()
        {
            await InitializeAsync();
            return await _database.Table<NotificationModel>().OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<NotificationModel> GetNotificationAsync(Guid id)
        {
            await InitializeAsync();
            return await _database.Table<NotificationModel>().Where(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task SaveNotificationAsync(NotificationModel notification)
        {
            await InitializeAsync();

            var existingNotification = await _database.Table<NotificationModel>()
                .Where(n => n.Id == notification.Id)
                .FirstOrDefaultAsync();

            if (existingNotification != null)
            {
                await _database.UpdateAsync(notification);
            }
            else
            {
                await _database.InsertAsync(notification);
            }
        }

        public async Task SaveNotificationsAsync(List<NotificationModel> notifications)
        {
            await InitializeAsync();

            foreach (var notification in notifications)
            {
                await SaveNotificationAsync(notification);
            }
        }

        public async Task MarkNotificationAsReadAsync(Guid id)
        {
            await InitializeAsync();

            var notification = await _database.Table<NotificationModel>()
                .Where(n => n.Id == id)
                .FirstOrDefaultAsync();

            if (notification != null)
            {
                notification.IsRead = true;
                await _database.UpdateAsync(notification);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync()
        {
            await InitializeAsync();

            var notifications = await _database.Table<NotificationModel>()
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                await _database.UpdateAsync(notification);
            }
        }
    }
}
