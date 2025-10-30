namespace Progra3_TPFinal_19B.Infrastructure;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _cs;

    public SqlConnectionFactory(IConfiguration cfg) => _cs = cfg.GetConnectionString("CallCenterDb")!;

    public IDbConnection CreateConnection()
        => new SqlConnection(_cs);
}
