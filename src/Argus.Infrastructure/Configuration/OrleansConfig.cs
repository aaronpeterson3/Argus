namespace Argus.Infrastructure.Configuration;

public sealed class OrleansConfig(
    string clusterId,
    string serviceId,
    int siloPort = 11111,
    int gatewayPort = 30000)
{
    public string ClusterId { get; init; } = clusterId;
    public string ServiceId { get; init; } = serviceId;
    public int SiloPort { get; init; } = siloPort;
    public int GatewayPort { get; init; } = gatewayPort;
    
    public readonly record struct PasswordHashConfig(
        int IterationCount = 100000,
        int NumBytesRequested = 32);
}