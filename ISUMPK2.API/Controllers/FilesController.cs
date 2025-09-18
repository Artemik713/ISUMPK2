using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

namespace ISUMPK2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesController> _logger;
        private readonly string _uploadsPath;

        public FilesController(IWebHostEnvironment environment, ILogger<FilesController> logger)
        {
            _environment = environment;
            _logger = logger;
            _uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "chat");

            // Создаем папки если их нет
            Directory.CreateDirectory(_uploadsPath);
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "thumbnails"));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] IFormFileCollection files, [FromForm] string metadata = "")
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("Файлы не выбраны");
                }

                var uploadedFiles = new List<object>();
                var maxFileSize = 10 * 1024 * 1024; // 10MB
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar" };

                foreach (var file in files)
                {
                    // Проверки
                    if (file.Length > maxFileSize)
                    {
                        return BadRequest($"Файл {file.FileName} слишком большой (макс. 10MB)");
                    }

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest($"Тип файла {extension} не поддерживается");
                    }

                    // Генерируем уникальное имя файла
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(_uploadsPath, uniqueFileName);

                    // Сохраняем файл
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Создаем thumbnail для изображений
                    string thumbnailPath = null;
                    if (IsImage(extension))
                    {
                        thumbnailPath = await CreateThumbnail(filePath, uniqueFileName);
                    }

                    var fileInfo = new
                    {
                        Id = Guid.NewGuid(),
                        FileName = file.FileName,
                        UniqueFileName = uniqueFileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        FilePath = $"/api/files/download/{uniqueFileName}",
                        ThumbnailPath = thumbnailPath != null ? $"/api/files/thumbnail/{Path.GetFileName(thumbnailPath)}" : null
                    };

                    uploadedFiles.Add(fileInfo);
                    _logger.LogInformation($"Файл {file.FileName} успешно загружен как {uniqueFileName}");
                }

                return Ok(new { success = true, files = uploadedFiles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файлов");
                return StatusCode(500, "Ошибка сервера при загрузке файлов");
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Файл не найден");
                }

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка скачивания файла {fileName}");
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpGet("thumbnail/{fileName}")]
        public async Task<IActionResult> GetThumbnail(string fileName)
        {
            try
            {
                var thumbnailPath = Path.Combine(_uploadsPath, "thumbnails", fileName);

                if (!System.IO.File.Exists(thumbnailPath))
                {
                    return NotFound("Thumbnail не найден");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(thumbnailPath);
                return File(fileBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения thumbnail {fileName}");
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpGet("preview/{fileName}")]
        public async Task<IActionResult> PreviewFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Файл не найден");
                }

                var extension = Path.GetExtension(fileName).ToLowerInvariant();

                // Для изображений возвращаем как inline
                if (IsImage(extension))
                {
                    var provider = new FileExtensionContentTypeProvider();
                    provider.TryGetContentType(fileName, out var contentType);

                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    return File(fileBytes, contentType ?? "image/jpeg");
                }

                // Для других файлов перенаправляем на скачивание
                return RedirectToAction("DownloadFile", new { fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка предпросмотра файла {fileName}");
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Удаляем thumbnail если есть
                var thumbnailPath = Path.Combine(_uploadsPath, "thumbnails", $"thumb_{fileName}");
                if (System.IO.File.Exists(thumbnailPath))
                {
                    System.IO.File.Delete(thumbnailPath);
                }

                return Ok(new { success = true, message = "Файл удален" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка удаления файла {fileName}");
                return StatusCode(500, "Ошибка сервера");
            }
        }

        private bool IsImage(string extension)
        {
            return new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" }.Contains(extension);
        }

        private async Task<string> CreateThumbnail(string originalPath, string fileName)
        {
            try
            {
                // Простое создание thumbnail (можно улучшить с помощью ImageSharp)
                var thumbnailFileName = $"thumb_{fileName}";
                var thumbnailPath = Path.Combine(_uploadsPath, "thumbnails", thumbnailFileName);

                // Пока просто копируем оригинал как thumbnail
                // В реальном проекте здесь нужно использовать библиотеку для ресайза
                System.IO.File.Copy(originalPath, thumbnailPath, true);

                return thumbnailPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания thumbnail");
                return null;
            }
        }

        [HttpGet("info/{fileName}")]
        public async Task<IActionResult> GetFileInfo(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Файл не найден");
                }

                var fileInfo = new FileInfo(filePath);
                var provider = new FileExtensionContentTypeProvider();
                provider.TryGetContentType(fileName, out var contentType);

                var result = new
                {
                    FileName = fileName,
                    Size = fileInfo.Length,
                    ContentType = contentType ?? "application/octet-stream",
                    CreatedAt = fileInfo.CreationTime,
                    LastModified = fileInfo.LastWriteTime,
                    Extension = Path.GetExtension(fileName),
                    IsImage = IsImage(Path.GetExtension(fileName).ToLowerInvariant())
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения информации о файле {fileName}");
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}