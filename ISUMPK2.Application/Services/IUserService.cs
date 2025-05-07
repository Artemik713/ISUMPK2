using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Application.Services
{
    public interface IUserService
    {
        Task<UserDto> GetCurrentUserAsync();
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(Guid departmentId);
        Task<UserDto> CreateUserAsync(UserCreateDto userDto);
        Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto);
        Task DeleteUserAsync(Guid id);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto loginDto);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<IEnumerable<string>> GetRolesAsync(Guid userId);
        Task<bool> IsInRoleAsync(Guid userId, string role);
        Task AddToRoleAsync(Guid userId, string role);
        Task RemoveFromRoleAsync(Guid userId, string role);
    }
}
