using System;
using System.Collections.Generic;
namespace ISUMPK2.Web.Models
{
    public class UserLoginResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
