using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;

namespace CallCenter.Business.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _cs;
        public CustomerRepository(string cs = null)
        {
            _cs = cs ?? ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
        }

        public bool ExistsByDocumentOrEmail(string document, string email)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT 1 FROM dbo.Customers
WHERE (Document = @Doc OR Email = @Email) AND IsDeleted = 0;", cn))
                {
                    cmd.Parameters.Add("@Doc", SqlDbType.NVarChar, 50).Value = document;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = email;
                    object o = cmd.ExecuteScalar();
                    return o != null;
                }
            }
        }

        public int Insert(Customer c)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.Customers (Name, Document, Email, Phone, Address)
OUTPUT inserted.Id
VALUES (@Name, @Doc, @Email, @Phone, @Addr);", cn))
                {
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = c.Name;
                    cmd.Parameters.Add("@Doc", SqlDbType.NVarChar, 50).Value = c.Document;
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = c.Email;
                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value = (object)c.Phone ?? DBNull.Value;
                    cmd.Parameters.Add("@Addr", SqlDbType.NVarChar, 300).Value = (object)c.Address ?? DBNull.Value;

                    object o = cmd.ExecuteScalar();
                    return o == null ? 0 : Convert.ToInt32(o);
                }
            }
        }
    }
}
