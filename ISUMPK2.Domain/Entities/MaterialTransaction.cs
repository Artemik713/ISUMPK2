using System;

namespace ISUMPK2.Domain.Entities
{
    public class MaterialTransaction : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // "Receipt" или "Issue"
        public Guid? TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Material Material { get; set; }
        public WorkTask Task { get; set; }
        public User User { get; set; }
    }
}
