using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [HttpPost("images")]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                // Проверяем, есть ли файлы в запросе
                if (Request.Form.Files.Count == 0)
                    return BadRequest("Файл не был отправлен");

                var file = Request.Form.Files[0];
                if (file.Length == 0)
                    return BadRequest("Файл пуст");

                // Создаем уникальное имя файла
                var fileName = $"uploaded_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

                // Проверяем наличие WebRootPath
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    // Для некоторых конфигураций WebRootPath может быть не установлен
                    _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // Создаем путь для сохранения
                var folderPath = Path.Combine(_environment.WebRootPath, "images", "products");

                // Убедимся, что директория существует
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host.Value}";

                // Возвращаем абсолютный URL
                return Ok($"{baseUrl}/images/products/{fileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

    }
}