using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Entities
{
    public class MaterialCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }

        // Навигационные свойства
        public MaterialCategory ParentCategory { get; set; }
        public ICollection<MaterialCategory> Subcategories { get; set; }
        public ICollection<Material> Materials { get; set; }
    }

}
