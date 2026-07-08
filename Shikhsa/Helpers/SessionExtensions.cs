using Shikhsa.ViewModels;
using System.Text.Json;

namespace Shikhsa.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(
            this ISession session,
            string key,
            T value)
        {
            session.SetString(
                key,
                JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(
            this ISession session,
            string key)
        {
            var value = session.GetString(key);

            return value == null
                ? default
                : JsonSerializer.Deserialize<T>(value);
        }
        public static UserSessionVM? GetCurrentUser(
             this ISession session)
        {
            return session.GetObject<UserSessionVM>("CurrentUser");
        }

        public static bool HasEditPermission(
                PermissionCacheVM cache,
                string controller)
            {
                return cache.Permissions.Any(x =>
                    x.ControllerName == controller &&
                    x.CanUpdate);
            }
        
    }
}