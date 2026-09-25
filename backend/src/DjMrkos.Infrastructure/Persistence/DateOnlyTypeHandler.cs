using System.Data;
using Dapper;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>
/// Dapper's own <c>LookupDbType</c> has no built-in mapping for <see cref="DateOnly"/> and
/// throws <see cref="NotSupportedException"/> the moment a query takes one as a parameter
/// (e.g. <c>Lead.EventDate</c>) — it never gets far enough to let Npgsql's native
/// <c>DateOnly</c> support kick in. A type handler sidesteps Dapper's lookup entirely and
/// hands the value to Npgsql directly, which maps it to <c>date</c> on its own.
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value) => parameter.Value = value;

    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}
