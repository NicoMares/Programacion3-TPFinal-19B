// CallCenter.Domain/Entities/User.cs
using System;

namespace CallCenter.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }                 // uniqueidentifier
        public string Username { get; set; }         // nvarchar(100)
        public string FullName { get; set; }         // nvarchar(200)
        public string Role { get; set; }             // nvarchar(20) (Administrador | Supervisor | Telefonista | Cliente)
        public string PasswordHash { get; set; }     // nvarchar(512)
        public DateTime CreatedAt { get; set; }      // datetime2(0)
        public Guid? CreatedByUserId { get; set; }   // uniqueidentifier NULL
        public DateTime? UpdatedAt { get; set; }     // datetime2(0) NULL
        public Guid? UpdatedByUserId { get; set; }   // uniqueidentifier NULL
        public bool IsDeleted { get; set; }          // bit
        public string Email { get; set; }            // nvarchar(256)
        public bool IsBlocked { get; set; }          // bit
    }
}
