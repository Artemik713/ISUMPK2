using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService,
                                      ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAllNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                // Явная фильтрация уведомлений по ID текущего пользователя
                var notifications = await _notificationService.GetAllNotificationsForUserAsync(userId);

                return Ok(notifications);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Пользователь не авторизован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении уведомлений");
                return StatusCode(500, "Ошибка при получении уведомлений");
            }
        }


        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUnreadNotificationsForUserAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return StatusCode(500, "Ошибка при получении непрочитанных уведомлений");
            }
        }

        [HttpGet("count-unread")]
        public async Task<ActionResult<int>> GetUnreadNotificationsCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadNotificationsCountForUserAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications count");
                return StatusCode(500, "Ошибка при получении количества непрочитанных уведомлений");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] NotificationCreateRequestDto request)
        {
            try
            {
                var notificationDto = new NotificationCreateDto
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    TaskId = request.TaskId
                };

                var notification = await _notificationService.CreateNotificationAsync(notificationDto);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, "Ошибка при создании уведомления");
            }
        }

        [HttpPost("users")]
        [Authorize(Roles = "Administrator,GeneralDirector")]
        public async Task<ActionResult> SendNotificationToMultipleUsers([FromBody] NotificationMultipleUsersDto request)
        {
            try
            {
                foreach (var userId in request.UserIds)
                {
                    var notificationDto = new NotificationCreateDto
                    {
                        UserId = userId,
                        Title = request.Title,
                        Message = request.Message,
                        TaskId = request.TaskId
                    };

                    await _notificationService.CreateNotificationAsync(notificationDto);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to multiple users");
                return StatusCode(500, "Ошибка при отправке уведомлений нескольким пользователям");
            }
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "Ошибка при отметке уведомления как прочитанного");
            }
        }

        [HttpPost("mark-all-as-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadForUserAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, "Ошибка при отметке всех уведомлений как прочитанных");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(Guid id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, "Ошибка при удалении уведомления");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован или ID некорректный");
            }
            return userId;
        }
    }

    // DTO для запроса на создание уведомления
    public class NotificationCreateRequestDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Guid? TaskId { get; set; }
    }

    // DTO для отправки уведомления нескольким пользователям
    public class NotificationMultipleUsersDto
    {
        public List<Guid> UserIds { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Guid? TaskId { get; set; }
    }
}
