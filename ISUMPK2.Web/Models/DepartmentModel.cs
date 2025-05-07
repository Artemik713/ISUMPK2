namespace ISUMPK2.Web.Models
{
    public class DepartmentModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? HeadId { get; set; }
        public string HeadName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
