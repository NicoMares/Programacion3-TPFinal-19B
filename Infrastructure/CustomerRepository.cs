// Infrastructure/CustomerRepository.cs
using System.Data;
using Progra3_TPFinal_19B.Application.Contracts;
using Progra3_TPFinal_19B.Models;
using Microsoft.Data.SqlClient;


namespace Progra3_TPFinal_19B.Infrastructure
{
    public sealed class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(IDbConnectionFactory f) : base(f) { }

        public Task<Customer?> GetByIdAsync(Guid id) =>
            QuerySingleAsync(@"
SELECT Id, DocumentNumber, Name, Email, Phone, CreatedAt, CreatedByUserId,
       UpdatedAt, UpdatedByUserId, IsDeleted
FROM Customers
WHERE Id = @Id AND IsDeleted = 0",
            Map, new { Id = id });

        public Task<IReadOnlyList<Customer>> ListAsync(int page, int size)
        {
            var offset = (page - 1) * size;
            return QueryAsync(@"
SELECT Id, DocumentNumber, Name, Email, Phone, CreatedAt, CreatedByUserId,
       UpdatedAt, UpdatedByUserId, IsDeleted
FROM Customers
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC
OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY",
            Map, new { Offset = offset, Size = size })
            .ContinueWith(t => (IReadOnlyList<Customer>)t.Result);
        }

        public Task CreateAsync(Customer c) =>
            ExecuteAsync(@"
INSERT INTO Customers (Id, DocumentNumber, Name, Email, Phone,
                       CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, IsDeleted)
VALUES (@Id, @DocumentNumber, @Name, @Email, @Phone,
        GETUTCDATE(), @CreatedByUserId, NULL, NULL, 0)", c);

        public Task UpdateAsync(Customer c) =>
            ExecuteAsync(@"
UPDATE Customers
SET DocumentNumber = @DocumentNumber,
    Name = @Name,
    Email = @Email,
    Phone = @Phone,
    UpdatedAt = GETUTCDATE(),
    UpdatedByUserId = @UpdatedByUserId
WHERE Id = @Id AND IsDeleted = 0", c);

        public Task DeleteAsync(Guid id, Guid updatedBy) =>
            ExecuteAsync(@"
UPDATE Customers
SET IsDeleted = 1,
    UpdatedAt = GETUTCDATE(),
    UpdatedByUserId = @UpdatedByUserId
WHERE Id = @Id",
            new { Id = id, UpdatedByUserId = updatedBy });

        private static Customer Map(IDataRecord r) => new()
        {
            Id = r.Get<Guid>("Id"),
            DocumentNumber = r.Get<string>("DocumentNumber"),
            Name = r.Get<string>("Name"),
            Email = r.Get<string>("Email"),
            Phone = r.Get<string?>("Phone"),
            CreatedAt = r.Get<DateTime>("CreatedAt"),
            CreatedByUserId = r.Get<Guid>("CreatedByUserId"),
            UpdatedAt = r.Get<DateTime?>("UpdatedAt"),
            UpdatedByUserId = r.Get<Guid?>("UpdatedByUserId"),
            IsDeleted = r.Get<bool>("IsDeleted")
        };
    

    public Task<bool> ExistsByDocumentAsync(string document) =>
    QuerySingleAsync<int?>(
        "SELECT 1 FROM Customers WHERE DocumentNumber=@D AND IsDeleted=0",
        _ => 1, new { D = document }
    ).ContinueWith(t => t.Result.HasValue);

        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null) =>
            QuerySingleAsync<int?>(
                @"SELECT 1 FROM Customers 
          WHERE Email=@E AND IsDeleted=0 AND (@Id IS NULL OR Id<>@Id)",
                _ => 1, new { E = email, Id = excludeId }
            ).ContinueWith(t => t.Result.HasValue);
    }
}
