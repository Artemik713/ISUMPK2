using System;
using SQLite;
namespace ISUMPK2.Mobile.Models
{
    public class NotificationModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public Guid? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public DateTime CreatedAt { get; set; }

        // Для локального хранения
        public bool IsLocal { get; set; }
        public bool IsSynced { get; set; }
    }
}
