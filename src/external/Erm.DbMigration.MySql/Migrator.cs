using System.Data;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Erm.DbMigration.MySql;

public static class MySqlMigrator
{
    public static async Task EnsureDatabaseCreated(string connectionString, ILogger logger, int timeout = -1)
    {
        if (string.IsNullOrEmpty(connectionString) || connectionString.Trim() == string.Empty)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The connection string does not specify a database name.");
        }

        connectionStringBuilder.Database = "sys";

        var maskedConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionStringBuilder.ConnectionString)
        {
            Password = string.Empty.PadRight(connectionStringBuilder.Password.Length, '*')
        };
        logger.LogInformation("Using connection string {ConnectionString}", maskedConnectionStringBuilder.ConnectionString);

        await using (var connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
        {
            try
            {
                await connection.OpenAsync();

                await using (var mySqlCommand = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS {databaseName}", connection) { CommandType = CommandType.Text })
                {
                    if (timeout >= 0)
                    {
                        mySqlCommand.CommandTimeout = timeout;
                    }

                    await mySqlCommand.ExecuteNonQueryAsync();
                }

                logger.LogInformation("Ensured database {DatabaseName} exists", databaseName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to open database connection to {ConnectionString}: {Database}",
                    maskedConnectionStringBuilder.ConnectionString, connection.Database);
                throw;
            }
        }
    }

    public static async Task RunSqlFiles(Assembly assembly, string connectionString, ILogger logger, int timeout = -1)
    {
        try
        {
            var sqlFiles = assembly.GetManifestResourceNames().Where(name => name.EndsWith(".sql")).ToList();

            await using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                foreach (var sqlFile in sqlFiles)
                {
                    await using (var stream = assembly.GetManifestResourceStream(sqlFile))
                    {
                        Debug.Assert(stream != null, nameof(stream) + " != null");

                        using (StreamReader reader = new(stream))
                        {
                            var sql = await reader.ReadToEndAsync();

                            await using (var command = conn.CreateCommand())
                            {
                                if (timeout >= 0)
                                {
                                    command.CommandTimeout = timeout;
                                }

                                command.CommandText = sql;
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
    }
}