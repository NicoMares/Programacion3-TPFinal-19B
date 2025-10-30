// Infrastructure/UserRepository.cs
using System.Data;
using System.Data.Common;
using Progra3_TPFinal_19B.Application.Contracts;
using Progra3_TPFinal_19B.Models; 

namespace Progra3_TPFinal_19B.Infrastructure
{
    public sealed class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IDbConnectionFactory f) : base(f) { }

        public Task<User?> FindForLoginAsync(string normalizedLogin) =>
            QuerySingleAsync(@"
SELECT Id, Username, FullName, Email, Role, IsBlocked, IsDeleted, CreatedAt, CreatedByUserId, UpdatedAt, PasswordHash
FROM Users
WHERE (Email = @Login OR Username = @Login) AND IsDeleted = 0",
            Map, new { Login = normalizedLogin });

        public async Task<bool> ExistsLoginAsync(string normalizedLogin)
        {
            const string sql = @"SELECT 1 FROM Users WHERE (Email=@Login OR Username=@Login) AND IsDeleted=0;";
            var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var p = cmd.CreateParameter(); p.ParameterName = "@Login"; p.Value = normalizedLogin; cmd.Parameters.Add(p);
            var obj = await (cmd as DbCommand)!.ExecuteScalarAsync();
            return obj != null;
        }

        public Task CreateAsync(User u) =>
            ExecuteAsync(@"
INSERT INTO Users (Id, Username, FullName, Email, Role, IsBlocked, IsDeleted, CreatedAt, CreatedByUserId, PasswordHash)
VALUES (@Id, @Username, @FullName, @Email, @Role, @IsBlocked, @IsDeleted, @CreatedAt, @CreatedByUserId, @PasswordHash);", u);

        public Task UpdatePasswordAsync(Guid userId, string newHexHash) =>
            ExecuteAsync(@"UPDATE Users SET PasswordHash=@Hash, UpdatedAt=GETUTCDATE() WHERE Id=@Id",
                new { Id = userId, Hash = newHexHash });

        private static User Map(IDataRecord r) => new()
        {
            Id = r.Get<Guid>(nameof(User.Id)),
            Username = r.Get<string>(nameof(User.Username)),
            FullName = r.Get<string?>(nameof(User.FullName)),
            Email = r.Get<string?>(nameof(User.Email)),
            Role = r.Get<string?>(nameof(User.Role)),
            IsBlocked = r.Get<bool>(nameof(User.IsBlocked)),
            IsDeleted = r.Get<bool>(nameof(User.IsDeleted)),
            CreatedAt = r.Get<DateTime>(nameof(User.CreatedAt)),
            CreatedByUserId = r.Get<Guid?>(nameof(User.CreatedByUserId)),
            UpdatedAt = r.Get<DateTime?>(nameof(User.UpdatedAt)),
            PasswordHash = r.Get<string>(nameof(User.PasswordHash))
        };
    }
}
