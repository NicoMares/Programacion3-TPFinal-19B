using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;

namespace CallCenter.Business.Repositories
{
    public class UserRepository : IUserRepository /* y si querés IUserReader */
    {
        private readonly string _cs;

        public UserRepository(string cs = null)
        {
            _cs = cs ?? ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
        }

        public User FindActiveByEmail(string email)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 Id, Username, FullName, [Role], PasswordHash,
       CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId,
       IsDeleted, Email, IsBlocked
FROM dbo.Users
WHERE Email = @Email AND IsDeleted = 0 AND IsBlocked = 0;", cn))
                {
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        User u = new User();
                        u.Id = rd.GetGuid(0);
                        u.Username = rd.GetString(1);
                        u.FullName = rd.GetString(2);
                        u.Role = rd.GetString(3);
                        u.PasswordHash = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        u.CreatedAt = rd.GetDateTime(5);
                        u.CreatedByUserId = rd.IsDBNull(6) ? (Guid?)null : rd.GetGuid(6);
                        u.UpdatedAt = rd.IsDBNull(7) ? (DateTime?)null : rd.GetDateTime(7);
                        u.UpdatedByUserId = rd.IsDBNull(8) ? (Guid?)null : rd.GetGuid(8);
                        u.IsDeleted = rd.GetBoolean(9);
                        u.Email = rd.GetString(10);
                        u.IsBlocked = rd.GetBoolean(11);
                        return u;
                    }
                }
            }
        }

        public bool ExistsByUsernameOrEmail(string username, string email)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT 1
FROM dbo.Users
WHERE (Username = @Username OR Email = @Email)
  AND IsDeleted = 0;", cn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                    object o = cmd.ExecuteScalar();
                    return o != null;
                }
            }
        }

        public Guid InsertTelefonista(User user)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.Users
(Id, Username, FullName, [Role], PasswordHash,
 CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId,
 IsDeleted, Email, IsBlocked)
OUTPUT inserted.Id
VALUES
(NEWID(), @Username, @FullName, N'Telefonista', @PasswordHash,
 SYSUTCDATETIME(), NULL, NULL, NULL,
 0, @Email, 0);", cn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = user.Username;
                    cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 200).Value = user.FullName;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = user.PasswordHash ?? "";
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = user.Email;

                    object o = cmd.ExecuteScalar();
                    return (o != null) ? (Guid)o : Guid.Empty;
                }
            }
        }

        public bool UpdatePassword(Guid userId, string newPasswordHash)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Users
SET PasswordHash = @Hash, UpdatedAt = SYSUTCDATETIME(), UpdatedByUserId = NULL
WHERE Id = @Id AND IsDeleted = 0;", cn))
                {
                    cmd.Parameters.Add("@Hash", SqlDbType.NVarChar, 512).Value = newPasswordHash;
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;
                    int n = cmd.ExecuteNonQuery();
                    return n > 0;
                }
            }
        }

        public User GetById(Guid id)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 Id, Username, FullName, [Role], PasswordHash,
       CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId,
       IsDeleted, Email, IsBlocked
FROM dbo.Users
WHERE Id = @Id;", cn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        User u = new User();
                        u.Id = rd.GetGuid(0);
                        u.Username = rd.GetString(1);
                        u.FullName = rd.GetString(2);
                        u.Role = rd.GetString(3);
                        u.PasswordHash = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        u.CreatedAt = rd.GetDateTime(5);
                        u.CreatedByUserId = rd.IsDBNull(6) ? (Guid?)null : rd.GetGuid(6);
                        u.UpdatedAt = rd.IsDBNull(7) ? (DateTime?)null : rd.GetDateTime(7);
                        u.UpdatedByUserId = rd.IsDBNull(8) ? (Guid?)null : rd.GetGuid(8);
                        u.IsDeleted = rd.GetBoolean(9);
                        u.Email = rd.GetString(10);
                        u.IsBlocked = rd.GetBoolean(11);
                        return u;
                    }
                }
            }
        }
    }

}
