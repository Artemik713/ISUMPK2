using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace ISUMPK2.Application.DTOs
{
    public class TaskMaterialDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
    }

    public class TaskMaterialCreateDto
    {
        public Guid TaskId { get; set; }
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class TaskMaterialUpdateDto
    {
        public decimal Quantity { get; set; }
    }
}