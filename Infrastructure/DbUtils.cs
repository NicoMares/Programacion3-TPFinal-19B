namespace Progra3_TPFinal_19B.Infrastructure;
using System;
using System.Data;

public static class DbUtils
{
    public static T Get<T>(this IDataRecord r, string col)
        => r[col] == DBNull.Value ? default! : (T)r[col];

    public static object DbVal(object? v) => v ?? DBNull.Value;
}
