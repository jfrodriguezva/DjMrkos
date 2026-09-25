using DjMrkos.Application.Common.Interfaces;

namespace DjMrkos.Infrastructure.Common;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
