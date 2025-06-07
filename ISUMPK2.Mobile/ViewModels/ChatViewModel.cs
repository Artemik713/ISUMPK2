using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Components.Pages;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ISUMPK2.Mobile.ViewModels
{
    [QueryProperty(nameof(ChatId), "id")]
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly IChatService _chatService;
        private readonly IChatRoomService _chatRoomService;
        private Guid _chatId;
        private string _newMessage;
        private bool _isLoading;
        private ObservableCollection<ChatMessageModel> _messages;
        private ChatRoomModel _chatRoom;

        public Guid ChatId
        {
            get => _chatId;
            set
            {
                if (_chatId != value)
                {
                    _chatId = value;
                    OnPropertyChanged();
                    LoadChatAsync().ConfigureAwait(false);
                }
            }
        }

        public string NewMessage
        {
            get => _newMessage;
            set
            {
                if (_newMessage != value)
                {
                    _newMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanSendMessage));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ChatMessageModel> Messages
        {
            get => _messages;
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChatRoomModel ChatRoom
        {
            get => _chatRoom;
            set
            {
                if (_chatRoom != value)
                {
                    _chatRoom = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Title => ChatRoom?.Name ?? "Чат";
        public bool CanSendMessage => !string.IsNullOrWhiteSpace(NewMessage);

        public ICommand SendMessageCommand { get; }
        public ICommand RefreshCommand { get; }

        public ChatViewModel(IChatService chatService, IChatRoomService chatRoomService)
        {
            _chatService = chatService;
            _chatRoomService = chatRoomService; // Новая зависимость

            SendMessageCommand = new Command(async () => await SendMessageAsync(), () => CanSendMessage);
            RefreshCommand = new Command(async () => await LoadChatAsync());

            Messages = new ObservableCollection<ChatMessageModel>();
        }

        private async Task LoadChatAsync()
        {
            if (ChatId == Guid.Empty) return;

            try
            {
                IsLoading = true;
                ChatRoom = await _chatRoomService.GetChatRoomAsync(ChatId);
                var messages = await _chatRoomService.GetChatMessagesAsync(ChatId);
                Messages = new ObservableCollection<ChatMessageModel>(messages);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить сообщения: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task SendMessageAsync()
        {
            if (!CanSendMessage) return;

            try
            {
                // Создаем DTO вместо модели
                var messageDto = new ChatMessageCreateDto
                {
                    Message = NewMessage,
                    ReceiverId = null, // Если сообщение для комнаты, а не пользователя
                    DepartmentId = ChatId // Используем ChatId как DepartmentId для комнатного чата
                };

                // Используем правильную сигнатуру метода
                var sentMessage = await _chatService.SendMessageAsync(
                    Guid.Empty, // ID отправителя (заменить на настоящий ID)
                    messageDto
                );

                NewMessage = string.Empty;

                // Добавляем отправленное сообщение в UI
                var messageModel = new ChatMessageModel
                {
                    Id = sentMessage.Id,
                    Content = sentMessage.Message,
                    ChatRoomId = ChatId,
                    SenderId = sentMessage.SenderId,
                    SenderName = sentMessage.SenderName,
                    SentAt = sentMessage.CreatedAt
                };

                Messages.Add(messageModel);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось отправить сообщение: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}