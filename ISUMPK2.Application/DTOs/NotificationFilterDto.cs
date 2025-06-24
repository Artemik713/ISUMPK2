using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Application.DTOs
{
    public class NotificationFilterDto
    {
        public Guid? UserId { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public Guid? TaskId { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string? Type { get; set; }
    }
}
