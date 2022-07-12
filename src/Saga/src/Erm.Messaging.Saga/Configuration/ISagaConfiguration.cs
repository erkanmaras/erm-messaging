using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.Messaging.Saga;

[PublicAPI]
public interface ISagaConfiguration
{
    IServiceCollection Services { get; }
    Assembly[]? ScanSagasIn { get; set; }
}