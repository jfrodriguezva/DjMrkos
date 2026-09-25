using DbUp;

// DJ MrKos does not use EF Core migrations — Dapper has no migration story of its own, so
// this small console app applies numbered SQL scripts (embedded from Scripts/) with DbUp.
// Run it once against a fresh database, and again after every deploy that adds a script.
//
// Usage: dotnet run --project src/DjMrkos.Migrator -- "Host=localhost;Database=djmrkos;Username=djmrkos;Password=..."

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("DJMRKOS_CONNECTION_STRING")
      ?? throw new InvalidOperationException(
          "Pasa la cadena de conexión como argumento o define DJMRKOS_CONNECTION_STRING.");

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly)
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return 1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Base de datos de DJ MrKos actualizada correctamente.");
Console.ResetColor();
return 0;
