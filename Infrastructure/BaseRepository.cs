namespace Progra3_TPFinal_19B.Infrastructure;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory _factory;
    protected BaseRepository(IDbConnectionFactory factory) => _factory = factory;

    protected async Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? tx = null)
    {
        var conn = tx?.Connection ?? _factory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        AddParams(cmd, parameters);
        return await (cmd as DbCommand)!.ExecuteNonQueryAsync();
    }

    protected async Task<T?> QuerySingleAsync<T>(string sql, System.Func<IDataReader, T> map, object? parameters = null, IDbTransaction? tx = null)
    {
        var conn = tx?.Connection ?? _factory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        AddParams(cmd, parameters);

        using var rd = await (cmd as DbCommand)!.ExecuteReaderAsync(CommandBehavior.SingleRow);
        return await rd.ReadAsync() ? map(rd) : default;
    }

    protected async Task<List<T>> QueryAsync<T>(string sql, System.Func<IDataReader, T> map, object? parameters = null, IDbTransaction? tx = null)
    {
        var conn = tx?.Connection ?? _factory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        AddParams(cmd, parameters);

        using var rd = await (cmd as DbCommand)!.ExecuteReaderAsync();
        var list = new List<T>();
        while (await rd.ReadAsync()) list.Add(map(rd));
        return list;
    }

    private static void AddParams(IDbCommand cmd, object? bag)
    {
        if (bag is null) return;
        foreach (var p in bag.GetType().GetProperties())
        {
            var prm = cmd.CreateParameter();
            prm.ParameterName = "@" + p.Name;
            prm.Value = DbUtils.DbVal(p.GetValue(bag));
            cmd.Parameters.Add(prm);
        }
    }
}
