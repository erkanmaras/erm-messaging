using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Saga;

internal class SagaConfiguration : ISagaConfiguration
{
    public IServiceCollection Services { get; }
    public Assembly[]? ScanSagasIn { get; set; }

    public SagaConfiguration(IServiceCollection services)
    {
        Services = services;
    }
}