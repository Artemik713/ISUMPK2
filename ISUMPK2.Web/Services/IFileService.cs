using ISUMPK2.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace ISUMPK2.Web.Services
{
    public interface IFileService
    {
        Task<List<ChatAttachmentModel>> UploadFilesAsync(IEnumerable<IBrowserFile> files);
        Task<bool> DeleteFileAsync(string fileName);
        Task<FileInfoModel?> GetFileInfoAsync(string fileName);
        string GetFileUrl(string fileName);
        string GetThumbnailUrl(string fileName);
        string GetPreviewUrl(string fileName);
    }
}