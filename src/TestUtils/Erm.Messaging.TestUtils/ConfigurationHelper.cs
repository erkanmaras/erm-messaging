using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Erm.Messaging.TestUtils;

public static class ConfigurationHelper
{
    public static IConfiguration BuildConfiguration(Assembly? userSecretsAssembly = null)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appsettings.json", optional: true, reloadOnChange: false);

        if (userSecretsAssembly != null)
        {
            builder.AddUserSecrets(userSecretsAssembly, optional: true);
        }

        return builder
            .AddEnvironmentVariables()
            .Build();
    }
}