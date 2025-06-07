using System;
using System.Collections.Generic;
namespace ISUMPK2.Mobile.Models
{
    public class UserModel
    {
        public Guid? Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }    
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public string Initials => string.IsNullOrEmpty(FirstName) ? "U" : FirstName.Substring(0, 1);
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
