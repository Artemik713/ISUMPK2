using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> PatchAsync<T>(this HttpClient client, string requestUri, object value)
        {
            var content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(requestUri, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}
