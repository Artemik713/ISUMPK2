using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Auth
{
    public interface IJwtTokenGenerator
    {
        Task<(string token, DateTime expiration)> GenerateTokenAsync(User user, IList<string> roles);
    }
}
