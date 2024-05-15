using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using Redis.OM.Modeling;

namespace CacheAdapters.CacheModels
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "Users" })]
    public class CacheUser
    {
        [RedisIdField][Indexed] public string? Id { get; set; }
        [Indexed] public string? Name { get; set; }
        [Indexed] public string? Email { get; set; } = null;
        [Indexed] public string? Password { get; set; }
        [Indexed] public UserRole? Role { get; set; }
        [Indexed] public string? Organization { get; set; } = null;
        [Indexed(Sortable = true)] public DateTimeOffset DeletionDateTime { get; set; }
        public CacheUser(User user, TimeSpan timeToLive)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Id = user.Id ?? throw new ArgumentException("Id field is null!", nameof(user));
            Name = user.Name ?? throw new ArgumentException("Name field is null!", nameof(user));
            Email = user.Email;
            Password = user.Password ?? throw new ArgumentException("Password field is null!", nameof(user));
            Role = user.Role! ?? throw new ArgumentException("Roles field is null!", nameof(user));
            Organization = user.Organization;
            DeletionDateTime = DateTimeOffset.UtcNow + timeToLive;
        }
        public CacheUser()
        {
                
        }
        public User ToDBUser()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Email = Email,
                Password = Password,
                Role = this.Role!
            };
        }
    }
}
