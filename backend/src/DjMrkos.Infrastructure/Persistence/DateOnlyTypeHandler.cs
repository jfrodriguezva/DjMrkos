using System.Data;
using Dapper;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>
/// Dapper's own <c>LookupDbType</c> has no built-in mapping for <see cref="DateOnly"/> and
/// throws <see cref="NotSupportedException"/> the moment a query takes one as a parameter
/// (e.g. <c>Lead.EventDate</c>) — it never gets far enough to let the SQL Server driver's
/// own <c>date</c> handling kick in. A type handler sidesteps Dapper's lookup entirely and
/// hands the value to the driver directly, which maps it to the <c>date</c> column type on its own.
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value) => parameter.Value = value;

    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}
