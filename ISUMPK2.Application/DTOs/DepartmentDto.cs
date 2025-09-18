using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ISUMPK2.Application.DTOs
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? HeadId { get; set; }

        public string HeadName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class DepartmentCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? HeadId { get; set; }
    }

    public class DepartmentUpdateDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? HeadId { get; set; }
    }
}