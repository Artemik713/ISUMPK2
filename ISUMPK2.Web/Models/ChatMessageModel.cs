namespace ISUMPK2.Web.Models
{
    public class ChatMessageModel
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public Guid? ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // НОВЫЕ ПОЛЯ ДЛЯ ФАЙЛОВ И ЗАДАЧ
        public List<ChatAttachmentModel> Attachments { get; set; } = new List<ChatAttachmentModel>();
        public Guid? RelatedTaskId { get; set; }
        public string RelatedTaskTitle { get; set; }
        public string MessageType { get; set; } = "text"; // text, image, task, emergency
    }
    public class ChatAttachmentModel
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string ThumbnailPath { get; set; }
    }
}
