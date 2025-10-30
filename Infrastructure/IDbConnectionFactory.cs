namespace Progra3_TPFinal_19B.Infrastructure;
using System.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
