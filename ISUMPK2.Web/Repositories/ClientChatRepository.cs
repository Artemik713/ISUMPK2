using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System.Net.Http;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Repositories
{
    public class ClientChatRepository : ClientRepositoryBase<ChatMessage>, IChatRepository
    {
        protected override string ApiEndpoint => "api/chat";

        public ClientChatRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesForUserAsync(Guid userId)
        {
            var response = await HttpClient.GetAsync($"{ApiEndpoint}/user/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessage>>() ?? Enumerable.Empty<ChatMessage>();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesForDepartmentAsync(Guid departmentId)
        {
            var response = await HttpClient.GetAsync($"{ApiEndpoint}/department/{departmentId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessage>>() ?? Enumerable.Empty<ChatMessage>();
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationAsync(Guid senderId, Guid receiverId)
        {
            var response = await HttpClient.GetAsync($"{ApiEndpoint}/conversation?senderId={senderId}&receiverId={receiverId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ChatMessage>>() ?? Enumerable.Empty<ChatMessage>();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var response = await HttpClient.PutAsync($"{ApiEndpoint}/{messageId}/mark-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            var response = await HttpClient.PutAsync($"{ApiEndpoint}/user/{userId}/mark-all-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetUnreadMessagesCountForUserAsync(Guid userId)
        {
            var response = await HttpClient.GetAsync($"{ApiEndpoint}/user/{userId}/unread-count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}