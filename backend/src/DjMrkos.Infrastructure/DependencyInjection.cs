using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Infrastructure.Common;
using DjMrkos.Infrastructure.Persistence;
using DjMrkos.Infrastructure.Qr;
using DjMrkos.Infrastructure.Realtime;
using DjMrkos.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DjMrkos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Postgres uses snake_case columns; the app's C# models stay PascalCase. This one line
        // is what lets every repository map `display_order` -> `DisplayOrder` for free.
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Dapper has no built-in DbType for DateOnly (used by Lead.EventDate) and throws
        // NotSupportedException without this — see DateOnlyTypeHandler's remarks.
        Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<QrOptions>(configuration.GetSection(QrOptions.SectionName));

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<IResilientDbExecutor, ResilientDbExecutor>();

        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISongRequestRepository, SongRequestRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IQrTokenService, QrTokenService>();
        services.AddScoped<ISongRequestNotifier, SignalRSongRequestNotifier>();

        services.AddSignalR();

        return services;
    }
}
