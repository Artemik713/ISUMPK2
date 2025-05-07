using System;
using System.Collections.Generic;
namespace ISUMPK2.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        // Навигационные свойства
        public ICollection<UserRole> UserRoles { get; set; }
        public Department Department { get; set; }
        public Guid? DepartmentId { get; set; }
        public ICollection<WorkTask> CreatedTasks { get; set; }
        public ICollection<WorkTask> AssignedTasks { get; set; }
        public ICollection<TaskComment> Comments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<ChatMessage> SentMessages { get; set; }
        public ICollection<ChatMessage> ReceivedMessages { get; set; }
        public ICollection<MaterialTransaction> MaterialTransactions { get; set; }
        public ICollection<ProductTransaction> ProductTransactions { get; set; }
    }
}
