using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace project.Helpers
{
    public static class SessionExtensions
    {
        // Phương thức lưu dữ liệu vào session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Phương thức lấy dữ liệu từ session
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
