using System;

namespace ISUMPK2.Domain.Entities
{
    public class ProductTransaction : BaseEntity
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; } // "Production" или "Shipment"
        public Guid? TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Notes { get; set; }

        // Навигационные свойства
        public Product Product { get; set; }
        public WorkTask Task { get; set; }
        public User User { get; set; }
    }
}
