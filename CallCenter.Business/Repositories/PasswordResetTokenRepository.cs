// CallCenter.Business/Repositories/PasswordResetTokenRepository.cs
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;

namespace CallCenter.Business.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly string _cs;

        public PasswordResetTokenRepository(string cs = null)
        {
            _cs = cs ?? ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
        }

        public Guid Create(Guid userId, string tokenHash, DateTime expiresAt)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.PasswordResetTokens (Id, UserId, TokenHash, ExpiresAt, Used, CreatedAt)
OUTPUT inserted.Id
VALUES (NEWID(), @UserId, @TokenHash, @ExpiresAt, 0, SYSUTCDATETIME());", cn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 64).Value = tokenHash;
                    cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = expiresAt;
                    object o = cmd.ExecuteScalar();
                    return (o != null) ? (Guid)o : Guid.Empty;
                }
            }
        }

        public PasswordResetToken GetValidByTokenHash(string tokenHash, DateTime nowUtc)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 Id, UserId, TokenHash, ExpiresAt, Used, CreatedAt
FROM dbo.PasswordResetTokens
WHERE TokenHash = @TokenHash AND Used = 0 AND ExpiresAt >= @Now;", cn))
                {
                    cmd.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 64).Value = tokenHash;
                    cmd.Parameters.Add("@Now", SqlDbType.DateTime2).Value = nowUtc;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        PasswordResetToken t = new PasswordResetToken();
                        t.Id = rd.GetGuid(0);
                        t.UserId = rd.GetGuid(1);
                        t.TokenHash = rd.GetString(2);
                        t.ExpiresAt = rd.GetDateTime(3);
                        t.Used = rd.GetBoolean(4);
                        t.CreatedAt = rd.GetDateTime(5);
                        return t;
                    }
                }
            }
        }

        public bool MarkUsed(Guid id)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.PasswordResetTokens SET Used = 1
WHERE Id = @Id;", cn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                    int n = cmd.ExecuteNonQuery();
                    return n > 0;
                }
            }
        }

        public bool InvalidateAllForUser(Guid userId)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.PasswordResetTokens SET Used = 1
WHERE UserId = @UserId AND Used = 0;", cn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
    }
}
