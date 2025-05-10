// ISUMPK2.Web/Auth/WebAssemblyJwtTokenGenerator.cs
using ISUMPK2.Application.Auth;
using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Auth
{
    public class WebAssemblyJwtTokenGenerator : IJwtTokenGenerator
    {
        public Task<(string token, DateTime expiration)> GenerateTokenAsync(User user, IList<string> roles)
        {
            // В WebAssembly нам не нужно генерировать токены, так как это делает сервер
            return Task.FromResult(("dummy-token", DateTime.UtcNow.AddDays(1)));
        }
    }
}
