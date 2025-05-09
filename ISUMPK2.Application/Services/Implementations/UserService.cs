using ISUMPK2.Application.Auth;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.EntityFrameworkCore;

namespace ISUMPK2.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        public UserService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IPasswordHasher<User> passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            var roles = await GetRolesAsync(id);
            return MapUserToDto(user, roles);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return null;

            var roles = await GetRolesAsync(user.Id);
            return MapUserToDto(user, roles);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await GetRolesAsync(user.Id);
                userDtos.Add(MapUserToDto(user, roles));
            }

            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _userRepository.GetUsersByRoleAsync(role);
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await GetRolesAsync(user.Id);
                userDtos.Add(MapUserToDto(user, roles));
            }

            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            var users = await _userRepository.GetUsersByDepartmentAsync(departmentId);
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await GetRolesAsync(user.Id);
                userDtos.Add(MapUserToDto(user, roles));
            }

            return userDtos;
        }
        public async Task<UserDto> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userGuid);
            if (user == null)
            {
                return null;
            }

            var roles = await GetRolesAsync(user.Id);
            return MapUserToDto(user, roles);
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto userDto)
        {
            try
            {
                // Проверяем существование пользователя
                var existingUser = await _userRepository.GetByUsernameAsync(userDto.UserName);
                if (existingUser != null)
                    throw new ApplicationException($"Пользователь с именем {userDto.UserName} уже существует.");

                if (!string.IsNullOrEmpty(userDto.Email))
                {
                    existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                    if (existingUser != null)
                        throw new ApplicationException($"Пользователь с email {userDto.Email} уже существует.");
                }

                // Предварительно проверяем существование всех ролей
                foreach (var roleName in userDto.Roles)
                {
                    var roleExists = await _userRepository.CheckRoleExistsAsync(roleName);
                    if (!roleExists)
                        throw new ApplicationException($"Роль {roleName} не найдена в системе");
                }


                // Создаем пользователя
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = userDto.UserName,
                    NormalizedUserName = userDto.UserName.ToUpper(),
                    Email = userDto.Email,
                    NormalizedEmail = userDto.Email?.ToUpper(),
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    MiddleName = userDto.MiddleName,
                    PhoneNumber = userDto.PhoneNumber,
                    DepartmentId = userDto.DepartmentId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = true, // Добавляем инициализацию обязательных полей
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                // Хешируем пароль
                user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _userRepository.AddAsync(user);
                        await _userRepository.SaveChangesAsync();

                        foreach (var role in userDto.Roles)
                        {
                            await _userRepository.AddToRoleAsync(user.Id, role);
                        }

                        await _userRepository.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new ApplicationException($"Ошибка при создании пользователя: {ex.Message}", ex);
                    }
                }


                // Получаем обновленные данные пользователя с ролями
                var refreshedUser = await _userRepository.GetByIdAsync(user.Id);
                var roles = await GetRolesAsync(user.Id);

                return MapUserToDto(refreshedUser, roles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании пользователя: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        public async Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ApplicationException($"User with ID {id} not found.");

            // Обновление данных пользователя
            user.Email = userDto.Email;
            user.NormalizedEmail = userDto.Email?.ToUpper();
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.MiddleName = userDto.MiddleName;
            user.PhoneNumber = userDto.PhoneNumber;
            user.DepartmentId = userDto.DepartmentId;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Обновление ролей
            var currentRoles = await GetRolesAsync(id);
            var rolesToRemove = currentRoles.Except(userDto.Roles);
            var rolesToAdd = userDto.Roles.Except(currentRoles);

            foreach (var role in rolesToRemove)
            {
                await _userRepository.RemoveFromRoleAsync(id, role);
            }

            foreach (var role in rolesToAdd)
            {
                await _userRepository.AddToRoleAsync(id, role);
            }

            await _userRepository.SaveChangesAsync();

            var updatedRoles = await GetRolesAsync(id);
            return MapUserToDto(user, updatedRoles);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ApplicationException($"User with ID {id} not found.");

            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.UserName);
            if (user == null)
                throw new ApplicationException("Invalid username or password.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
                throw new ApplicationException("Invalid username or password.");

            var roles = await GetRolesAsync(user.Id);
            var (token, expiration) = await _jwtTokenGenerator.GenerateTokenAsync(user, roles.ToList());

            return new UserLoginResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                Token = token,
                TokenExpiration = expiration
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ApplicationException($"User with ID {userId} not found.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return user.UserRoles?.Select(ur => ur.Role.Name) ?? new List<string>();
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            return await _userRepository.IsInRoleAsync(userId, role);
        }

        public async Task AddToRoleAsync(Guid userId, string role)
        {
            await _userRepository.AddToRoleAsync(userId, role);
        }

        public async Task RemoveFromRoleAsync(Guid userId, string role)
        {
            await _userRepository.RemoveFromRoleAsync(userId, role);
        }

        private UserDto MapUserToDto(User user, IEnumerable<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                PhoneNumber = user.PhoneNumber,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
