using System.Collections.Concurrent;
using System.Threading.Tasks;
using SV22T1020761.Models.Security;
using SV22T1020761.Shop.Services;

namespace SV22T1020761.Shop.Services
{
    public static class AccountService
    {
        private static readonly ConcurrentDictionary<string, UserAccount> users = new ConcurrentDictionary<string, UserAccount>();
        private static readonly ConcurrentDictionary<string, string> passwords = new ConcurrentDictionary<string, string>();

        public static Task<UserAccount?> ValidateUserAsync(string username, string password)
        {
            if (username == null) return Task.FromResult<UserAccount?>(null);
            if (users.TryGetValue(username.ToLowerInvariant(), out var user) && passwords.TryGetValue(username.ToLowerInvariant(), out var hash))
            {
                if (PasswordHasher.VerifyHashedPassword(hash, password))
                    return Task.FromResult<UserAccount?>(user);
            }
            return Task.FromResult<UserAccount?>(null);
        }

        public static UserAccount? GetUser(string username)
        {
            if (username == null) return null;
            users.TryGetValue(username.ToLowerInvariant(), out var user);
            return user;
        }

        public static void UpdateUser(UserAccount model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserName)) return;
            users.AddOrUpdate(model.UserName.ToLowerInvariant(), model, (k, v) => model);
        }

        public static bool ValidatePassword(string username, string password)
        {
            if (username == null) return false;
            if (passwords.TryGetValue(username.ToLowerInvariant(), out var hash))
            {
                return PasswordHasher.VerifyHashedPassword(hash, password);
            }
            return false;
        }

        public static void ChangePassword(string username, string newPassword)
        {
            if (username == null) return;
            var hash = PasswordHasher.HashPassword(newPassword);
            passwords.AddOrUpdate(username.ToLowerInvariant(), hash, (k, v) => hash);
        }

        public static Task RegisterAsync(UserAccount model, string password)
        {
            if (model == null || string.IsNullOrEmpty(model.UserName)) return Task.CompletedTask;
            model.UserId = System.Guid.NewGuid().ToString();
            users.TryAdd(model.UserName.ToLowerInvariant(), model);
            var hash = PasswordHasher.HashPassword(password);
            passwords.TryAdd(model.UserName.ToLowerInvariant(), hash);
            return Task.CompletedTask;
        }
    }
}
