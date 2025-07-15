using ISUMPK2.Web.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;

namespace ISUMPK2.Web.Services
{
    public class FileService : IFileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileService> _logger;

        public FileService(HttpClient httpClient, ILogger<FileService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ChatAttachmentModel>> UploadFilesAsync(IEnumerable<IBrowserFile> files)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(fileContent, "files", file.Name);
                }

                var response = await _httpClient.PostAsync("/api/files/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<UploadResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Files != null)
                    {
                        return result.Files.Select(f => new ChatAttachmentModel
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            FilePath = f.FilePath,
                            ContentType = f.ContentType,
                            FileSize = f.FileSize,
                            ThumbnailPath = f.ThumbnailPath
                        }).ToList();
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка загрузки файлов: {error}");
                }

                return new List<ChatAttachmentModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файлов");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/files/delete/{fileName}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка удаления файла {fileName}");
                return false;
            }
        }

        public async Task<FileInfoModel?> GetFileInfoAsync(string fileName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/files/info/{fileName}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiFileInfo = JsonSerializer.Deserialize<ApiFileInfo>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiFileInfo != null)
                    {
                        return new FileInfoModel
                        {
                            FileName = apiFileInfo.FileName,
                            Size = apiFileInfo.Size,
                            ContentType = apiFileInfo.ContentType,
                            CreatedAt = apiFileInfo.CreatedAt,
                            LastModified = apiFileInfo.LastModified,
                            Extension = apiFileInfo.Extension,
                            IsImage = apiFileInfo.IsImage
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения информации о файле {fileName}");
                return null;
            }
        }

        public string GetFileUrl(string fileName)
        {
            return $"/api/files/download/{fileName}";
        }

        public string GetThumbnailUrl(string fileName)
        {
            return $"/api/files/thumbnail/{fileName}";
        }

        public string GetPreviewUrl(string fileName)
        {
            return $"/api/files/preview/{fileName}";
        }

        // Приватные классы для десериализации ответов API
        private class UploadResponse
        {
            public bool Success { get; set; }
            public List<UploadedFile> Files { get; set; } = new();
        }

        private class UploadedFile
        {
            public Guid Id { get; set; }
            public string FileName { get; set; } = string.Empty;
            public string UniqueFileName { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string FilePath { get; set; } = string.Empty;
            public string ThumbnailPath { get; set; } = string.Empty;
        }

        // Переименовываем класс чтобы избежать конфликта с System.IO.FileInfo
        private class ApiFileInfo
        {
            public string FileName { get; set; } = string.Empty;
            public long Size { get; set; }
            public string ContentType { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public string Extension { get; set; } = string.Empty;
            public bool IsImage { get; set; }
        }
    }
}