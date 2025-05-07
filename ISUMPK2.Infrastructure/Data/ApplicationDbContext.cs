using ISUMPK2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskStatus = ISUMPK2.Domain.Entities.TaskStatus;

namespace ISUMPK2.Infrastructure.Data
{
    public class ApplicationDbContext :DbContext
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Role> Roles { get; set; }
            public DbSet<UserRole> UserRoles { get; set; }
            public DbSet<Department> Departments { get; set; }
            public DbSet<Material> Materials { get; set; }
            public DbSet<ProductType> ProductTypes { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductMaterial> ProductMaterials { get; set; }
            public DbSet<TaskStatus> TaskStatuses { get; set; }
            public DbSet<TaskPriority> TaskPriorities { get; set; }
            public DbSet<WorkTask> WorkTasks { get; set; } // Изменено с Tasks
            public DbSet<TaskComment> TaskComments { get; set; }
            public DbSet<MaterialTransaction> MaterialTransactions { get; set; }
            public DbSet<ProductTransaction> ProductTransactions { get; set; }
            public DbSet<Notification> Notifications { get; set; }
            public DbSet<ChatMessage> ChatMessages { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Настройка для WorkTask
                modelBuilder.Entity<WorkTask>()
                    .ToTable("Tasks"); ;
            // UserRole - составной первичный ключ
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // UserRole - отношения
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Department - отношения
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Head)
                .WithMany()
                .HasForeignKey(d => d.HeadId);

            // User - отношения
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedTasks)
                .WithOne(t => t.Creator)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedTasks)
                .WithOne(t => t.Assignee)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductMaterial - составной первичный ключ
            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            // ProductMaterial - отношения
            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId);

            // Task - отношения
            modelBuilder.Entity<Domain.Entities.WorkTask>()
                .HasOne(t => t.Status)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.StatusId);

            modelBuilder.Entity<Domain.Entities.WorkTask>()
                .HasOne(t => t.Priority)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.PriorityId);

            modelBuilder.Entity<Domain.Entities.WorkTask>()
                .HasOne(t => t.Department)
                .WithMany(d => d.Tasks)
                .HasForeignKey(t => t.DepartmentId);

            modelBuilder.Entity<Domain.Entities.WorkTask>()
                .HasOne(t => t.Product)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProductId);

            // ChatMessage - требуется либо ReceiverId, либо DepartmentId
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Department)
                .WithMany(d => d.ChatMessages)
                .HasForeignKey(cm => cm.DepartmentId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
