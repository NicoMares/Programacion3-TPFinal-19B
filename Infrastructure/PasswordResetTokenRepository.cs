// Infrastructure/PasswordResetTokenRepository.cs
using System;
using System.Data;
using System.Data.Common;
using Progra3_TPFinal_19B.Application.Contracts;
using Progra3_TPFinal_19B;
using ResetToken = Progra3_TPFinal_19B.Models.PasswordResetToken;

namespace Progra3_TPFinal_19B.Infrastructure
{
    public sealed class PasswordResetTokenRepository : BaseRepository, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(IDbConnectionFactory f) : base(f) { }

        public async Task CreateAsync(ResetToken t)
        {
            const string sql = @"
INSERT INTO PasswordResetTokens (Id, UserId, Token, ExpiresAt, UsedAt)
VALUES (@Id, @UserId, @Token, @ExpiresAt, NULL);";

            await ExecuteAsync(sql, new { t.Id, t.UserId, t.Token, t.ExpiresAt });
        }

        public async Task<bool> IsValidAsync(string token)
        {
            const string sql = @"
SELECT 1
FROM PasswordResetTokens
WHERE Token=@Token AND UsedAt IS NULL AND ExpiresAt > GETUTCDATE();";

            var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var p = cmd.CreateParameter();
            p.ParameterName = "@Token";
            p.Value = token;
            cmd.Parameters.Add(p);

            var result = await (cmd as DbCommand)!.ExecuteScalarAsync();
            return result != null;
        }

        public Task<ResetToken?> GetByTokenAsync(string token)
        {
            const string sql = @"
SELECT Id, UserId, Token, ExpiresAt, UsedAt
FROM PasswordResetTokens
WHERE Token=@Token";

            return QuerySingleAsync(sql, Map, new { Token = token });
        }

        public Task MarkUsedAsync(Guid id) =>
            ExecuteAsync(@"UPDATE PasswordResetTokens SET UsedAt=GETUTCDATE() WHERE Id=@Id", new { Id = id });

        private static ResetToken Map(IDataRecord r) => new()
        {
            Id = r.Get<Guid>(nameof(ResetToken.Id)),
            UserId = r.Get<Guid>(nameof(ResetToken.UserId)),
            Token = r.Get<string>(nameof(ResetToken.Token)),
            ExpiresAt = r.Get<DateTime>(nameof(ResetToken.ExpiresAt)),
            UsedAt = r.Get<DateTime?>(nameof(ResetToken.UsedAt))
        };
    }
}
